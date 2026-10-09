using EcommerceAPI.Extensions;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using EcommerceAPI.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoRepository _repository;

        public ProductoController(IProductoRepository repository)
        {
            _repository = repository;
        }

        // Público. Si el usuario es Admin puede pedir también los inactivos.
        [HttpGet("Obtener-Producto")]
        public async Task<ActionResult<PagedResult<ProductoDto>>> GetAll(
            [FromQuery] ProductoFiltroDto filtro, [FromQuery] bool incluirInactivos = false)
        {
            var verInactivos = incluirInactivos && User.IsInRole(Roles.Admin);
            return Ok(await _repository.BuscarAsync(filtro, verInactivos));
        }

        [HttpGet("Obtener-Producto/{id:int}")]
        public async Task<ActionResult<ProductoDto>> GetById(int id)
        {
            var producto = await _repository.GetDtoByIdAsync(id);
            if (producto == null || (!producto.Activo && !User.IsInRole(Roles.Admin)))
                return NotFound(new { mensaje = "Producto no encontrado" });

            return Ok(producto);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost("Crear-producto")]
        public async Task<ActionResult<ProductoDto>> Create([FromBody] ProductoGuardarDto dto)
        {
            var creado = await _repository.CrearAsync(dto, User.GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPut("Actualizar-Producto/{id:int}")]
        public async Task<ActionResult<ProductoDto>> Update(int id, [FromBody] ProductoGuardarDto dto)
        {
            var actualizado = await _repository.ActualizarAsync(id, dto);
            if (actualizado == null)
                return NotFound(new { mensaje = "Producto no encontrado" });

            return Ok(actualizado);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("Eliminar-producto/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminado = await _repository.EliminarAsync(id);
            if (!eliminado)
                return NotFound(new { mensaje = "Producto no encontrado" });

            return NoContent();
        }
    }
}