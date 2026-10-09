using EcommerceAPI.Models.Dtos;

namespace EcommerceAPI.Interfaces
{
    public interface IPedidoService
    {
        Task<PedidoDto> CheckoutAsync(int userId, CheckoutDto dto);
        Task<List<PedidoDto>> MisPedidosAsync(int userId);
        Task<PedidoDto> ObtenerAsync(int id, int userId, bool esAdmin);
        Task<List<PedidoDto>> TodosAsync(string? estado);
        Task<PedidoDto> CambiarEstadoAsync(int id, string estado);
        Task<PedidoDto> CambiarEstadoPagoAsync(int id, string estado);
        Task<PedidoDto> CancelarAsync(int id, int userId);
    }
}