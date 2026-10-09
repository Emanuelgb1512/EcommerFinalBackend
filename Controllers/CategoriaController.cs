using EcommerceAPI.DB;
using EcommerceAPI.Models;
using EcommerceAPI.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("Listar")]
        public async Task<ActionResult<List<CategoriaDto>>> Listar()
        {
            var categorias = await _context.Categoria.AsNoTracking()
                .OrderBy(c => c.Nombre)
                .Select(c => new CategoriaDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    CantidadProductos = c.Productos.Count(p => p.Activo)
                })
                .ToListAsync();
            return Ok(categorias);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost("Crear")]
        public async Task<ActionResult<CategoriaDto>> Crear([FromBody] CategoriaGuardarDto dto)
        {
            var nombre = dto.Nombre.Trim();
            if (await _context.Categoria.AnyAsync(c => c.Nombre == nombre))
                throw new AppException("Ya existe una categoría con ese nombre.", 409);

            var categoria = new Categoria { Nombre = nombre, Descripcion = dto.Descripcion?.Trim() };
            _context.Categoria.Add(categoria);
            await _context.SaveChangesAsync();

            return Ok(new CategoriaDto { Id = categoria.Id, Nombre = categoria.Nombre, Descripcion = categoria.Descripcion });
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPut("Actualizar/{id:int}")]
        public async Task<ActionResult<CategoriaDto>> Actualizar(int id, [FromBody] CategoriaGuardarDto dto)
        {
            var categoria = await _context.Categoria.FindAsync(id);
            if (categoria == null) return NotFound(new { mensaje = "Categoría no encontrada" });

            var nombre = dto.Nombre.Trim();
            if (await _context.Categoria.AnyAsync(c => c.Nombre == nombre && c.Id != id))
                throw new AppException("Ya existe una categoría con ese nombre.", 409);

            categoria.Nombre = nombre;
            categoria.Descripcion = dto.Descripcion?.Trim();
            await _context.SaveChangesAsync();

            return Ok(new CategoriaDto { Id = categoria.Id, Nombre = categoria.Nombre, Descripcion = categoria.Descripcion });
        }

        // Los productos de la categoría borrada quedan "sin categoría"
        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("Eliminar/{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var categoria = await _context.Categoria.FindAsync(id);
            if (categoria == null) return NotFound(new { mensaje = "Categoría no encontrada" });

            _context.Categoria.Remove(categoria);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}