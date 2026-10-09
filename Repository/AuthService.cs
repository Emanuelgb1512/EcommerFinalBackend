using EcommerceAPI.DB;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using EcommerceAPI.Models.Dtos;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EcommerceAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtSettings _jwt;
        private readonly GoogleSettings _google;
        private readonly string? _adminEmail;
        private readonly PasswordHasher<Usuario> _hasher = new();

        public AuthService(AppDbContext context, IOptions<JwtSettings> jwt,
            IOptions<GoogleSettings> google, IConfiguration config)
        {
            _context = context;
            _jwt = jwt.Value;
            _google = google.Value;
            _adminEmail = config["AdminEmail"];
        }

        public async Task<AuthResponseDto> RegistrarAsync(RegisterDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            if (await _context.Usuario.AnyAsync(u => u.Email == email))
                throw new AppException("Ya existe una cuenta con ese correo.", 409);

            var usuario = new Usuario
            {
                Nombre = dto.Nombre.Trim(),
                Email = email,
                Rol = EsAdminEmail(email) ? Roles.Admin : Roles.Cliente
            };
            usuario.Password = _hasher.HashPassword(usuario, dto.Password);

            _context.Usuario.Add(usuario);
            await _context.SaveChangesAsync();
            return Respuesta(usuario);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null)
                throw new AppException("Correo o contraseña incorrectos.", 401);

            if (string.IsNullOrEmpty(usuario.Password))
                throw new AppException("Esta cuenta se creó con Google. Inicia sesión con Google.", 400);

            var resultado = _hasher.VerifyHashedPassword(usuario, usuario.Password, dto.Password);
            if (resultado == PasswordVerificationResult.Failed)
                throw new AppException("Correo o contraseña incorrectos.", 401);

            if (!usuario.Activo)
                throw new AppException("Tu cuenta está desactivada.", 403);

            await PromoverSiEsAdminAsync(usuario);
            return Respuesta(usuario);
        }

        public async Task<AuthResponseDto> GoogleAsync(GoogleLoginDto dto)
        {
            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(dto.Credential,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _google.ClientId }
                    });
            }
            catch (InvalidJwtException)
            {
                throw new AppException("No se pudo validar la cuenta de Google.", 401);
            }

            if (!payload.EmailVerified)
                throw new AppException("El correo de Google no está verificado.", 401);

            var email = payload.Email.ToLowerInvariant();
            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.GoogleId == payload.Subject || u.Email == email);

            if (usuario == null)
            {
                usuario = new Usuario
                {
                    Nombre = string.IsNullOrWhiteSpace(payload.Name) ? email : payload.Name,
                    Email = email,
                    GoogleId = payload.Subject,
                    FotoUrl = payload.Picture,
                    Rol = EsAdminEmail(email) ? Roles.Admin : Roles.Cliente
                };
                _context.Usuario.Add(usuario);
            }
            else
            {
                if (!usuario.Activo)
                    throw new AppException("Tu cuenta está desactivada.", 403);
                usuario.GoogleId ??= payload.Subject;
                if (EsAdminEmail(email)) usuario.Rol = Roles.Admin;
            }

            await _context.SaveChangesAsync();
            return Respuesta(usuario);
        }

        // ===== Auxiliares =====
        private bool EsAdminEmail(string email) =>
            !string.IsNullOrWhiteSpace(_adminEmail) &&
            string.Equals(email, _adminEmail.Trim(), StringComparison.OrdinalIgnoreCase);

        private async Task PromoverSiEsAdminAsync(Usuario usuario)
        {
            if (EsAdminEmail(usuario.Email) && usuario.Rol != Roles.Admin)
            {
                usuario.Rol = Roles.Admin;
                await _context.SaveChangesAsync();
            }
        }

        private AuthResponseDto Respuesta(Usuario u) => new()
        {
            Token = GenerarToken(u),
            Usuario = new UsuarioInfoDto
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Email = u.Email,
                Rol = u.Rol,
                Telefono = u.Telefono,
                Direccion = u.Direccion,
                Ciudad = u.Ciudad,
                Activo = u.Activo,
                FechaRegistro = u.FechaRegistro,
                FotoUrl = u.FotoUrl
            }
        };

        private string GenerarToken(Usuario u)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, u.Id.ToString()),
                new(ClaimTypes.Name, u.Nombre),
                new(ClaimTypes.Email, u.Email),
                new(ClaimTypes.Role, u.Rol)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}