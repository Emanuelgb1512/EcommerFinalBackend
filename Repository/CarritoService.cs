using EcommerceAPI.DB;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using EcommerceAPI.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Services
{
    public class CarritoService : ICarritoService
    {
        private readonly AppDbContext _context;

        public CarritoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CarritoDto> ObtenerAsync(int userId)
        {
            var items = await _context.DetalleCarrito.AsNoTracking()
                .Where(d => d.Carrito!.UsuarioId == userId)
                .OrderBy(d => d.Id)
                .Select(d => new CarritoItemDto
                {
                    ProductoId = d.ProductoId,
                    Nombre = d.Producto!.Nombre,
                    ImagenUrl = d.Producto.ImagenUrl,
                    PrecioUnitario = d.Producto.Precio,
                    Cantidad = d.Cantidad,
                    Stock = d.Producto.Stock,
                    Activo = d.Producto.Activo
                })
                .ToListAsync();

            return new CarritoDto { Items = items };
        }

        public async Task<CarritoDto> AgregarAsync(int userId, int productoId, int cantidad)
        {
            await AgregarInternoAsync(userId, productoId, cantidad);
            return await ObtenerAsync(userId);
        }

        public async Task<CarritoDto> ActualizarCantidadAsync(int userId, int productoId, int cantidad)
        {
            var detalle = await _context.DetalleCarrito
                .Include(d => d.Producto)
                .FirstOrDefaultAsync(d => d.Carrito!.UsuarioId == userId && d.ProductoId == productoId)
                ?? throw new AppException("El producto no está en tu carrito.", 404);

            if (!detalle.Producto!.Activo)
                throw new AppException("Este producto ya no está disponible.", 400);
            if (cantidad > detalle.Producto.Stock)
                throw new AppException($"Solo quedan {detalle.Producto.Stock} unidades disponibles.", 400);

            detalle.Cantidad = cantidad;
            await _context.SaveChangesAsync();
            return await ObtenerAsync(userId);
        }

        public async Task<CarritoDto> QuitarAsync(int userId, int productoId)
        {
            var detalle = await _context.DetalleCarrito
                .FirstOrDefaultAsync(d => d.Carrito!.UsuarioId == userId && d.ProductoId == productoId);

            if (detalle != null)
            {
                _context.DetalleCarrito.Remove(detalle);
                await _context.SaveChangesAsync();
            }
            return await ObtenerAsync(userId);
        }

        public async Task VaciarAsync(int userId)
        {
            var detalles = await _context.DetalleCarrito
                .Where(d => d.Carrito!.UsuarioId == userId).ToListAsync();
            _context.DetalleCarrito.RemoveRange(detalles);
            await _context.SaveChangesAsync();
        }

        // Une el carrito de invitado (navegador) con el de la base de datos al iniciar sesión.
        // Los productos sin stock o inactivos se omiten sin fallar.
        public async Task<CarritoDto> SincronizarAsync(int userId, List<AgregarCarritoDto> items)
        {
            foreach (var item in items)
            {
                try { await AgregarInternoAsync(userId, item.ProductoId, item.Cantidad); }
                catch (AppException) { /* se omite */ }
            }
            return await ObtenerAsync(userId);
        }

        private async Task AgregarInternoAsync(int userId, int productoId, int cantidad)
        {
            var producto = await _context.Producto.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productoId && p.Activo)
                ?? throw new AppException("Producto no disponible.", 404);

            var carrito = await _context.Carrito.FirstOrDefaultAsync(c => c.UsuarioId == userId);
            if (carrito == null)
            {
                carrito = new Carrito { UsuarioId = userId };
                _context.Carrito.Add(carrito);
                await _context.SaveChangesAsync();
            }

            var detalle = await _context.DetalleCarrito
                .FirstOrDefaultAsync(d => d.CarritoId == carrito.Id && d.ProductoId == productoId);

            var nueva = (detalle?.Cantidad ?? 0) + cantidad;
            if (nueva > producto.Stock)
                throw new AppException($"Solo quedan {producto.Stock} unidades disponibles.", 400);

            if (detalle == null)
                _context.DetalleCarrito.Add(new DetalleCarrito
                {
                    CarritoId = carrito.Id,
                    ProductoId = productoId,
                    Cantidad = nueva
                });
            else
                detalle.Cantidad = nueva;

            await _context.SaveChangesAsync();
        }
    }
}