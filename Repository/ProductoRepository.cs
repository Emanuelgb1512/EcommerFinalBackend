using EcommerceAPI.DB;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Repository
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly AppDbContext _context;

        public ProductoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Producto>> GetAllAsync()
        {
            return await _context.Producto.AsNoTracking().ToListAsync();
        }

        public async Task<Producto?> GetByIdAsync(int id)
        {
            return await _context.Producto.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Producto> AddAsync(Producto producto, int userid)
        {
            producto.UsuarioId = userid;
            _context.Producto.Add(producto);
            await _context.SaveChangesAsync();
            return producto;
        }

        public async Task UpdateAsync(Producto producto, int userid)
        {
            producto.UsuarioId = userid;
            _context.Producto.Update(producto);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Producto producto, int userid)
        {
            producto.UsuarioId = userid;
            _context.Producto.Remove(producto);
            await _context.SaveChangesAsync();
        }
    }
}