using EcommerceAPI.Models.Dtos;

namespace EcommerceAPI.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegistrarAsync(RegisterDto dto);
        Task<AuthResponseDto> RegistrarAdminAsync(RegisterAdminDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> GoogleAsync(GoogleLoginDto dto);
    }
}