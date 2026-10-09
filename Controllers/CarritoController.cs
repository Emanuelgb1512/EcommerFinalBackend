using EcommerceAPI.Extensions;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CarritoController : ControllerBase
    {
        private readonly ICarritoService _carrito;

        public CarritoController(ICarritoService carrito)
        {
            _carrito = carrito;
        }

        [HttpGet]
        public async Task<ActionResult<CarritoDto>> Obtener()
            => Ok(await _carrito.ObtenerAsync(User.GetUserId()));

        [HttpPost("Agregar")]
        public async Task<ActionResult<CarritoDto>> Agregar([FromBody] AgregarCarritoDto dto)
            => Ok(await _carrito.AgregarAsync(User.GetUserId(), dto.ProductoId, dto.Cantidad));

        [HttpPut("Cantidad/{productoId:int}")]
        public async Task<ActionResult<CarritoDto>> Cantidad(int productoId, [FromBody] CantidadDto dto)
            => Ok(await _carrito.ActualizarCantidadAsync(User.GetUserId(), productoId, dto.Cantidad));

        [HttpDelete("Quitar/{productoId:int}")]
        public async Task<ActionResult<CarritoDto>> Quitar(int productoId)
            => Ok(await _carrito.QuitarAsync(User.GetUserId(), productoId));

        [HttpDelete("Vaciar")]
        public async Task<IActionResult> Vaciar()
        {
            await _carrito.VaciarAsync(User.GetUserId());
            return NoContent();
        }

        [HttpPost("Sincronizar")]
        public async Task<ActionResult<CarritoDto>> Sincronizar([FromBody] List<AgregarCarritoDto> items)
            => Ok(await _carrito.SincronizarAsync(User.GetUserId(), items));
    }
}