using EcommerceAPI.DB;
using EcommerceAPI.Extensions;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using EcommerceAPI.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICloudinaryService _cloudinary;

        public UsuarioController(AppDbContext context, ICloudinaryService cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        private static UsuarioInfoDto Mapear(Usuario u) => new()
        {
            Id = u.Id,
            Nombre = u.Nombre,
            Email = u.Email,
            Rol = u.Rol,
            Telefono = u.Telefono,
            Direccion = u.Direccion,
            Ciudad = u.Ciudad,
            FotoUrl = u.FotoUrl,
            Activo = u.Activo,
            FechaRegistro = u.FechaRegistro
        };

        // ===== Perfil del usuario logueado =====
        [HttpGet("Perfil")]
        public async Task<ActionResult<UsuarioInfoDto>> ObtenerPerfil()
        {
            var u = await _context.Usuario.AsNoTracking().FirstOrDefaultAsync(x => x.Id == User.GetUserId());
            if (u == null) return NotFound(new { mensaje = "Usuario no encontrado" });
            return Ok(Mapear(u));
        }

        [HttpPut("Perfil")]
        public async Task<ActionResult<UsuarioInfoDto>> ActualizarPerfil([FromBody] PerfilDto dto)
        {
            var u = await _context.Usuario.FirstOrDefaultAsync(x => x.Id == User.GetUserId());
            if (u == null) return NotFound(new { mensaje = "Usuario no encontrado" });

            u.Nombre = dto.Nombre.Trim();
            u.Telefono = dto.Telefono?.Trim();
            u.Direccion = dto.Direccion?.Trim();
            u.Ciudad = dto.Ciudad?.Trim();
            await _context.SaveChangesAsync();
            return Ok(Mapear(u));
        }

        // ===== Foto de perfil =====
        [HttpPut("Perfil/Foto")]
        public async Task<ActionResult<UsuarioInfoDto>> SubirFoto([FromBody] FotoPerfilDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ImagenBase64))
                throw new AppException("Debes enviar una imagen.", 400);
            if (dto.ImagenBase64.Length > 4_000_000)
                throw new AppException("La imagen es demasiado pesada.", 400);

            var u = await _context.Usuario.FirstOrDefaultAsync(x => x.Id == User.GetUserId());
            if (u == null) return NotFound(new { mensaje = "Usuario no encontrado" });

            // Primero sube la nueva; la anterior se borra solo si todo salió bien
            (string Url, string PublicId) foto;
            try { foto = await _cloudinary.UploadAvatarAsync(dto.ImagenBase64); }
            catch (ArgumentException ex) { throw new AppException(ex.Message, 400); }

            var anterior = u.FotoPublicId;
            u.FotoUrl = foto.Url;
            u.FotoPublicId = foto.PublicId;
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(anterior))
                await _cloudinary.DeleteImageAsync(anterior);

            return Ok(Mapear(u));
        }

        [HttpDelete("Perfil/Foto")]
        public async Task<ActionResult<UsuarioInfoDto>> QuitarFoto()
        {
            var u = await _context.Usuario.FirstOrDefaultAsync(x => x.Id == User.GetUserId());
            if (u == null) return NotFound(new { mensaje = "Usuario no encontrado" });

            var anterior = u.FotoPublicId;
            u.FotoUrl = null;
            u.FotoPublicId = null;
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(anterior))
                await _cloudinary.DeleteImageAsync(anterior);

            return Ok(Mapear(u));
        }

        [HttpPut("Cambiar-Password")]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
        {
            var u = await _context.Usuario.FirstOrDefaultAsync(x => x.Id == User.GetUserId());
            if (u == null) return NotFound(new { mensaje = "Usuario no encontrado" });

            var hasher = new PasswordHasher<Usuario>();

            // Las cuentas creadas con Google no tienen contraseña: pueden establecer una sin la actual
            if (!string.IsNullOrEmpty(u.Password))
            {
                if (string.IsNullOrEmpty(dto.PasswordActual) ||
                    hasher.VerifyHashedPassword(u, u.Password, dto.PasswordActual) == PasswordVerificationResult.Failed)
                    throw new AppException("La contraseña actual no es correcta.", 400);
            }

            u.Password = hasher.HashPassword(u, dto.PasswordNueva);
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Contraseña actualizada." });
        }

        // ===== Administración de usuarios =====
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("Listar")]
        public async Task<ActionResult<List<UsuarioInfoDto>>> Listar([FromQuery] string? busqueda)
        {
            var q = _context.Usuario.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var b = busqueda.Trim();
                q = q.Where(u => u.Nombre.Contains(b) || u.Email.Contains(b));
            }
            var usuarios = await q.OrderByDescending(u => u.FechaRegistro).ToListAsync();
            return Ok(usuarios.Select(Mapear).ToList());
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{id:int}/Rol")]
        public async Task<ActionResult<UsuarioInfoDto>> CambiarRol(int id, [FromBody] CambiarRolDto dto)
        {
            if (dto.Rol != Roles.Admin && dto.Rol != Roles.Cliente)
                throw new AppException("Rol no válido.", 400);
            if (id == User.GetUserId())
                throw new AppException("No puedes cambiar tu propio rol.", 400);

            var u = await _context.Usuario.FindAsync(id);
            if (u == null) return NotFound(new { mensaje = "Usuario no encontrado" });

            u.Rol = dto.Rol;
            await _context.SaveChangesAsync();
            return Ok(Mapear(u));
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{id:int}/Activo")]
        public async Task<ActionResult<UsuarioInfoDto>> CambiarActivo(int id, [FromBody] CambiarActivoDto dto)
        {
            if (id == User.GetUserId())
                throw new AppException("No puedes desactivar tu propia cuenta.", 400);

            var u = await _context.Usuario.FindAsync(id);
            if (u == null) return NotFound(new { mensaje = "Usuario no encontrado" });

            u.Activo = dto.Activo;
            await _context.SaveChangesAsync();
            return Ok(Mapear(u));
        }
    }
}