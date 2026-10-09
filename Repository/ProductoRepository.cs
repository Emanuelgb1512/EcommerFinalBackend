using EcommerceAPI.DB;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using EcommerceAPI.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Repository
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly AppDbContext _context;
        private readonly ICloudinaryService _cloudinary;

        public ProductoRepository(AppDbContext context, ICloudinaryService cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        public async Task<PagedResult<ProductoDto>> BuscarAsync(ProductoFiltroDto f, bool incluirInactivos)
        {
            var q = _context.Producto.AsNoTracking().AsQueryable();

            if (!incluirInactivos) q = q.Where(p => p.Activo);

            if (!string.IsNullOrWhiteSpace(f.Busqueda))
            {
                var b = f.Busqueda.Trim();
                q = q.Where(p => p.Nombre.Contains(b) || p.Descripcion.Contains(b));
            }
            if (f.CategoriaId.HasValue) q = q.Where(p => p.CategoriaId == f.CategoriaId);
            if (f.PrecioMin.HasValue) q = q.Where(p => p.Precio >= f.PrecioMin);
            if (f.PrecioMax.HasValue) q = q.Where(p => p.Precio <= f.PrecioMax);

            q = f.Orden switch
            {
                "precio_asc" => q.OrderBy(p => p.Precio),
                "precio_desc" => q.OrderByDescending(p => p.Precio),
                "nombre" => q.OrderBy(p => p.Nombre),
                _ => q.OrderByDescending(p => p.Id)
            };

            var pagina = Math.Max(1, f.Pagina);
            var tamano = Math.Clamp(f.TamanoPagina, 1, 60);
            var total = await q.CountAsync();

            var items = await q.Skip((pagina - 1) * tamano).Take(tamano)
                .Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    ImagenUrl = p.ImagenUrl,
                    Activo = p.Activo,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria != null ? p.Categoria.Nombre : null
                })
                .ToListAsync();

            return new PagedResult<ProductoDto>
            {
                Items = items,
                Total = total,
                Pagina = pagina,
                TamanoPagina = tamano
            };
        }

        public async Task<ProductoDto?> GetDtoByIdAsync(int id)
        {
            return await _context.Producto.AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    ImagenUrl = p.ImagenUrl,
                    Activo = p.Activo,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria != null ? p.Categoria.Nombre : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ProductoDto> CrearAsync(ProductoGuardarDto dto, int userId)
        {
            await ValidarCategoriaAsync(dto.CategoriaId);

            var producto = new Producto
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion?.Trim() ?? string.Empty,
                Precio = dto.Precio,
                Stock = dto.Stock,
                CategoriaId = dto.CategoriaId,
                Activo = dto.Activo,
                UsuarioId = userId
            };

            if (!string.IsNullOrWhiteSpace(dto.ImagenBase64))
            {
                var (url, publicId) = await SubirImagenAsync(dto.ImagenBase64);
                producto.ImagenUrl = url;
                producto.ImagenPublicId = publicId;
            }

            _context.Producto.Add(producto);
            await _context.SaveChangesAsync();
            return (await GetDtoByIdAsync(producto.Id))!;
        }

        public async Task<ProductoDto?> ActualizarAsync(int id, ProductoGuardarDto dto)
        {
            var producto = await _context.Producto.FirstOrDefaultAsync(p => p.Id == id);
            if (producto == null) return null;

            await ValidarCategoriaAsync(dto.CategoriaId);

            producto.Nombre = dto.Nombre.Trim();
            producto.Descripcion = dto.Descripcion?.Trim() ?? string.Empty;
            producto.Precio = dto.Precio;
            producto.Stock = dto.Stock;
            producto.CategoriaId = dto.CategoriaId;
            producto.Activo = dto.Activo;

            string? publicIdAnterior = null;
            if (!string.IsNullOrWhiteSpace(dto.ImagenBase64))
            {
                // primero sube la nueva; la anterior se borra solo si todo salió bien
                var (url, publicId) = await SubirImagenAsync(dto.ImagenBase64);
                publicIdAnterior = producto.ImagenPublicId;
                producto.ImagenUrl = url;
                producto.ImagenPublicId = publicId;
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(publicIdAnterior))
                await _cloudinary.DeleteImageAsync(publicIdAnterior);

            return await GetDtoByIdAsync(id);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var producto = await _context.Producto.FirstOrDefaultAsync(p => p.Id == id);
            if (producto == null) return false;

            // Si ya fue vendido, se desactiva para conservar el historial de pedidos
            var vendido = await _context.DetallePedido.AnyAsync(d => d.ProductoId == id);
            if (vendido)
            {
                producto.Activo = false;
                await _context.SaveChangesAsync();
                return true;
            }

            if (!string.IsNullOrEmpty(producto.ImagenPublicId))
                await _cloudinary.DeleteImageAsync(producto.ImagenPublicId);

            _context.Producto.Remove(producto);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task ValidarCategoriaAsync(int? categoriaId)
        {
            if (categoriaId.HasValue && !await _context.Categoria.AnyAsync(c => c.Id == categoriaId))
                throw new AppException("La categoría seleccionada no existe.", 400);
        }

        private async Task<(string Url, string PublicId)> SubirImagenAsync(string base64)
        {
            try { return await _cloudinary.UploadImageAsync(base64); }
            catch (ArgumentException ex) { throw new AppException(ex.Message, 400); }
        }
    }
}