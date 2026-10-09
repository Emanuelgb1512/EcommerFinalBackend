using EcommerceAPI.DB;
using EcommerceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Authorize(Roles = Roles.Admin)]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("Resumen")]
        public async Task<IActionResult> Resumen()
        {
            var ventas = await _context.Pedido
                .Where(p => p.Estado != EstadosPedido.Cancelado)
                .SumAsync(p => (decimal?)p.Total) ?? 0;

            var porEstado = await _context.Pedido
                .GroupBy(p => p.Estado)
                .Select(g => new { estado = g.Key, cantidad = g.Count() })
                .ToListAsync();

            var stockBajo = await _context.Producto
                .Where(p => p.Activo && p.Stock <= 5)
                .OrderBy(p => p.Stock).Take(10)
                .Select(p => new { p.Id, p.Nombre, p.Stock })
                .ToListAsync();

            return Ok(new
            {
                ventasTotales = ventas,
                totalPedidos = await _context.Pedido.CountAsync(),
                totalUsuarios = await _context.Usuario.CountAsync(),
                totalProductos = await _context.Producto.CountAsync(p => p.Activo),
                pedidosPorEstado = porEstado,
                productosStockBajo = stockBajo
            });
        }
    }
}