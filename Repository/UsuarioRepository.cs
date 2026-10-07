using EcommerceAPI.DB;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using EcommerceAPI.Models.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EcommerceAPI.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public UsuarioRepository(AppDbContext context, IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<string> Login(LoginDto item)
        {
            var usuario = await _context.Usuario.FirstOrDefaultAsync(x => x.Email == item.Email);

            if (usuario == null)
            {
                return "Usuario o contraseña incorrectos.";
            }
            var passwordHasher = new PasswordHasher<Usuario>();
            var resultado = passwordHasher.VerifyHashedPassword(usuario,
            usuario.Password,
            item.Password);
            if (resultado == PasswordVerificationResult.Failed)
            {
                return "Usuario o contraseña incorrectos.";
            }
            return GenerarToken(usuario);
        }

        public async Task<string> Registrar(UsuarioDto item)
        {
            var usuarioExiste = await _context.Usuario.FirstOrDefaultAsync(x => x.Email == item.Email);
            if (usuarioExiste != null)
            {
                return "El usuario ya existe.";
            }
            var usuario = new Usuario
            {
                Nombre = item.Nombre,
                Email = item.Email
            };
            var passwordHasher = new PasswordHasher<Usuario>();
            usuario.Password = passwordHasher.HashPassword(usuario, item.Password);
            await _context.Usuario.AddAsync(usuario);
            await _context.SaveChangesAsync();
            return "Usuario registrado correctamente.";
        }

        private string GenerarToken(Usuario usuario)
        {
            var claims = new List<Claim>{
                 new Claim(
                 ClaimTypes.NameIdentifier,
                 usuario.Id.ToString()),
                 new Claim(
                 ClaimTypes.Name,
                 usuario.Nombre),
                 new Claim(
                 ClaimTypes.Email,
                 usuario.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
            _jwtSettings.DurationInMinutes),
            signingCredentials: credentials);
            return new JwtSecurityTokenHandler()
            .WriteToken(token);
        }
    }
}
