using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using EcommerceAPI.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoRepository _repository;
        private readonly ICloudinaryService _cloudinary;

        public ProductoController(IProductoRepository repository, ICloudinaryService cloudinary)
        {
            _repository = repository;
            _cloudinary = cloudinary;
        }

        [HttpGet("Obtener-Producto")]
        public async Task<ActionResult<List<Producto>>> GetAll()
        {
            var productos = await _repository.GetAllAsync();
            return Ok(productos);
        }

        [HttpGet("Obtener-Producto/{id}")]
        public async Task<ActionResult<Producto>> GetById([FromRoute]int id)
        {
            var producto = await _repository.GetByIdAsync(id);
            if (producto == null)
                return NotFound(new { mensaje = "Producto no encontrado" });

            return Ok(producto);
        }

        [Authorize]
        [HttpPost("Crear-producto")]
        public async Task<ActionResult<Producto>> Create([FromBody] Producto producto)
        {
            var error = Validar(producto);
            if (error != null)
                return BadRequest(new { mensaje = error });

            producto.Id = 0;
            producto.ImagenUrl = null;
            producto.ImagenPublicId = null;
            
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var creado = await _repository.AddAsync(producto, userId);
            return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
        }

        [Authorize]
        [HttpPut("Actualizar-Producto/{id}")]
        public async Task<IActionResult> Update([FromRoute]int id, [FromBody] Producto datos)
        {
            var producto = await _repository.GetByIdAsync(id);
            if (producto == null)
                return NotFound(new { mensaje = "Producto no encontrado" });

            var error = Validar(datos);
            if (error != null)
                return BadRequest(new { mensaje = error });

            producto.Nombre = datos.Nombre;
            producto.Descripcion = datos.Descripcion;
            producto.Precio = datos.Precio;
            producto.Stock = datos.Stock;

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _repository.UpdateAsync(producto, userId);
            return Ok(producto);
        }

        [Authorize]
        [HttpDelete("Eliminar-producto/{id}")]
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            var producto = await _repository.GetByIdAsync(id);
            if (producto == null)
                return NotFound(new { mensaje = "Producto no encontrado" });

            if (!string.IsNullOrEmpty(producto.ImagenPublicId))
                await _cloudinary.DeleteImageAsync(producto.ImagenPublicId);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _repository.DeleteAsync(producto, userId);
            return NoContent();
        }

        [Authorize]
        [HttpPost("Imagen del producto/{id}")]
        public async Task<IActionResult> SubirImagen(int id, [FromBody] ImagenDto dto)
        {
            var producto = await _repository.GetByIdAsync(id);
            if (producto == null)
                return NotFound(new { mensaje = "Producto no encontrado" });

            if (string.IsNullOrWhiteSpace(dto.ImagenBase64))
                return BadRequest(new { mensaje = "Debes enviar la imagen en Base64" });

            try
            {
                if (!string.IsNullOrEmpty(producto.ImagenPublicId))
                    await _cloudinary.DeleteImageAsync(producto.ImagenPublicId);

                var (url, publicId) = await _cloudinary.UploadImageAsync(dto.ImagenBase64);
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);


                producto.ImagenUrl = url;
                producto.ImagenPublicId = publicId;
                await _repository.UpdateAsync(producto, userId);

                return Ok(producto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        private static string? Validar(Producto p)
        {
            if (string.IsNullOrWhiteSpace(p.Nombre))
                return "El nombre no puede estar vacío";
            if (p.Precio < 0)
                return "El precio no puede ser negativo";
            if (p.Stock < 0)
                return "El stock no puede ser negativo";
            return null;
        }
    }
}