using EcommerceAPI.Models.Dtos;

namespace EcommerceAPI.Interfaces
{
    public interface ICarritoService
    {
        Task<CarritoDto> ObtenerAsync(int userId);
        Task<CarritoDto> AgregarAsync(int userId, int productoId, int cantidad);
        Task<CarritoDto> ActualizarCantidadAsync(int userId, int productoId, int cantidad);
        Task<CarritoDto> QuitarAsync(int userId, int productoId);
        Task VaciarAsync(int userId);
        Task<CarritoDto> SincronizarAsync(int userId, List<AgregarCarritoDto> items);
    }
}