using EcommerceAPI.Interfaces;
using EcommerceAPI.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("{controller}")]
    public class AuthController : Controller
    {
        private readonly IUsuarioRepository _usuarioRepository;
        public AuthController(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UsuarioDto item)
        {
            return Ok(await _usuarioRepository.Registrar(item));
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto item)
        {
            return Ok(await _usuarioRepository.Login(item));
        }
    }
}
