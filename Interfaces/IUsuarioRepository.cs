using EcommerceAPI.Models.Dtos;

namespace EcommerceAPI.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<string> Registrar(UsuarioDto item);
        Task<string> Login(LoginDto item);
    }

}
