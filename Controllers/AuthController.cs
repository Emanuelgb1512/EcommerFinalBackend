using EcommerceAPI.Interfaces;
using EcommerceAPI.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
            => Ok(await _auth.RegistrarAsync(dto));

        [HttpPost("Login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
            => Ok(await _auth.LoginAsync(dto));

        [HttpPost("Google")]
        public async Task<ActionResult<AuthResponseDto>> Google([FromBody] GoogleLoginDto dto)
            => Ok(await _auth.GoogleAsync(dto));
    }
}