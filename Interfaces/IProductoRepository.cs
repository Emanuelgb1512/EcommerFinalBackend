using EcommerceAPI.Models;
using EcommerceAPI.Models.Dtos;

namespace EcommerceAPI.Interfaces
{
    public interface IProductoRepository
    {
        Task<PagedResult<ProductoDto>> BuscarAsync(ProductoFiltroDto filtro, bool incluirInactivos);
        Task<ProductoDto?> GetDtoByIdAsync(int id);
        Task<ProductoDto> CrearAsync(ProductoGuardarDto dto, int userId);
        Task<ProductoDto?> ActualizarAsync(int id, ProductoGuardarDto dto);
        Task<bool> EliminarAsync(int id);
    }
}