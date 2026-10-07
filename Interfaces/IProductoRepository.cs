using EcommerceAPI.Models;
namespace EcommerceAPI.Interfaces
{
    public interface IProductoRepository
    {
        Task<List<Producto>> GetAllAsync();
        Task<Producto?> GetByIdAsync(int id);
        Task<Producto> AddAsync(Producto producto, int userid);
        Task UpdateAsync(Producto producto, int userid);
        Task DeleteAsync(Producto producto, int userid);
    }
}