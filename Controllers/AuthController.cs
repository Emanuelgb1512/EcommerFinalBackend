using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using EcommerceAPI.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private const int MaxIntentosAdmin = 5;
        private static readonly TimeSpan Bloqueo = TimeSpan.FromMinutes(15);

        private readonly IAuthService _auth;
        private readonly IMemoryCache _cache;

        public AuthController(IAuthService auth, IMemoryCache cache)
        {
            _auth = auth;
            _cache = cache;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
            => Ok(await _auth.RegistrarAsync(dto));

        // Crea una cuenta de administrador. Exige la clave especial.
        [HttpPost("Register-Admin")]
        public async Task<ActionResult<AuthResponseDto>> RegisterAdmin([FromBody] RegisterAdminDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
            var llave = $"admin-reg:{ip}";
            var intentos = _cache.Get<int>(llave);

            if (intentos >= MaxIntentosAdmin)
                throw new AppException("Demasiados intentos fallidos. Intenta de nuevo en unos minutos.", 429);

            try
            {
                var respuesta = await _auth.RegistrarAdminAsync(dto);
                _cache.Remove(llave);
                return Ok(respuesta);
            }
            catch (AppException ex) when (ex.StatusCode == 403)
            {
                // Clave incorrecta: cuenta como intento fallido
                _cache.Set(llave, intentos + 1, Bloqueo);
                throw;
            }
        }

        [HttpPost("Login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
            => Ok(await _auth.LoginAsync(dto));

        [HttpPost("Google")]
        public async Task<ActionResult<AuthResponseDto>> Google([FromBody] GoogleLoginDto dto)
            => Ok(await _auth.GoogleAsync(dto));
    }
}