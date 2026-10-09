using EcommerceAPI.Extensions;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using EcommerceAPI.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PedidoController : ControllerBase
    {
        private readonly IPedidoService _pedidos;

        public PedidoController(IPedidoService pedidos)
        {
            _pedidos = pedidos;
        }

        [HttpPost("Checkout")]
        public async Task<ActionResult<PedidoDto>> Checkout([FromBody] CheckoutDto dto)
            => Ok(await _pedidos.CheckoutAsync(User.GetUserId(), dto));

        [HttpGet("Mis-Pedidos")]
        public async Task<ActionResult<List<PedidoDto>>> MisPedidos()
            => Ok(await _pedidos.MisPedidosAsync(User.GetUserId()));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PedidoDto>> Obtener(int id)
            => Ok(await _pedidos.ObtenerAsync(id, User.GetUserId(), User.IsInRole(Roles.Admin)));

        [HttpPost("{id:int}/Cancelar")]
        public async Task<ActionResult<PedidoDto>> Cancelar(int id)
            => Ok(await _pedidos.CancelarAsync(id, User.GetUserId()));

        [Authorize(Roles = Roles.Admin)]
        [HttpGet("Todos")]
        public async Task<ActionResult<List<PedidoDto>>> Todos([FromQuery] string? estado)
            => Ok(await _pedidos.TodosAsync(estado));

        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{id:int}/Estado")]
        public async Task<ActionResult<PedidoDto>> CambiarEstado(int id, [FromBody] CambiarEstadoDto dto)
            => Ok(await _pedidos.CambiarEstadoAsync(id, dto.Estado));

        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{id:int}/Pago")]
        public async Task<ActionResult<PedidoDto>> CambiarEstadoPago(int id, [FromBody] CambiarEstadoPagoDto dto)
            => Ok(await _pedidos.CambiarEstadoPagoAsync(id, dto.Estado));
    }
}