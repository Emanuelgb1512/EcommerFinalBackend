using EcommerceAPI.DB;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using EcommerceAPI.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Services
{
    public class PedidoService : IPedidoService
    {
        private static readonly string[] MetodosValidos = { "Tarjeta", "PSE", "Contraentrega" };
        private readonly AppDbContext _context;

        public PedidoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PedidoDto> CheckoutAsync(int userId, CheckoutDto dto)
        {
            if (!MetodosValidos.Contains(dto.MetodoPago))
                throw new AppException("Método de pago no válido.", 400);

            await using var tx = await _context.Database.BeginTransactionAsync();

            var carrito = await _context.Carrito
                .Include(c => c.Detalles).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (carrito == null || carrito.Detalles.Count == 0)
                throw new AppException("Tu carrito está vacío.", 400);

            var pedido = new Pedido
            {
                UsuarioId = userId,
                DireccionEnvio = dto.DireccionEnvio.Trim(),
                Ciudad = dto.Ciudad.Trim(),
                Telefono = dto.Telefono.Trim(),
                Fecha = DateTime.UtcNow
            };

            foreach (var d in carrito.Detalles)
            {
                var p = d.Producto!;
                if (!p.Activo)
                    throw new AppException($"\"{p.Nombre}\" ya no está disponible.", 400);
                if (d.Cantidad > p.Stock)
                    throw new AppException($"No hay stock suficiente de \"{p.Nombre}\" (quedan {p.Stock}).", 400);

                p.Stock -= d.Cantidad;
                pedido.Detalles.Add(new DetallePedido
                {
                    ProductoId = p.Id,
                    NombreProducto = p.Nombre,
                    PrecioUnitario = p.Precio,
                    Cantidad = d.Cantidad,
                    Subtotal = p.Precio * d.Cantidad
                });
            }

            // Subtotal + IVA = Total
            pedido.Subtotal = pedido.Detalles.Sum(x => x.Subtotal);
            pedido.Impuestos = Math.Round(pedido.Subtotal * Tarifas.Iva, 0, MidpointRounding.AwayFromZero);
            pedido.Total = pedido.Subtotal + pedido.Impuestos;

            // ====== PAGO SIMULADO ======
            // Aquí se integraría Wompi / Mercado Pago.
            var (estadoPago, estadoPedido) = dto.MetodoPago switch
            {
                "Tarjeta" => (EstadosPago.Aprobado, EstadosPedido.Pagado),
                "PSE" => (EstadosPago.EnProceso, EstadosPedido.Pendiente),
                _ => (EstadosPago.Pendiente, EstadosPedido.Pendiente)   // Contraentrega
            };

            pedido.Pago = new Pago
            {
                Metodo = dto.MetodoPago,
                Monto = pedido.Total,
                Estado = estadoPago,
                Referencia = "SIM-" + Guid.NewGuid().ToString("N")[..12].ToUpper(),
                Fecha = DateTime.UtcNow
            };
            pedido.Estado = estadoPedido;
            // ===========================

            _context.Pedido.Add(pedido);
            _context.DetalleCarrito.RemoveRange(carrito.Detalles);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return await ObtenerAsync(pedido.Id, userId, true);
        }

        public async Task<List<PedidoDto>> MisPedidosAsync(int userId)
        {
            var pedidos = await Consulta().Where(p => p.UsuarioId == userId)
                .OrderByDescending(p => p.Fecha).ToListAsync();
            return pedidos.Select(Mapear).ToList();
        }

        public async Task<PedidoDto> ObtenerAsync(int id, int userId, bool esAdmin)
        {
            var pedido = await Consulta().FirstOrDefaultAsync(p => p.Id == id);
            if (pedido == null || (!esAdmin && pedido.UsuarioId != userId))
                throw new AppException("Pedido no encontrado.", 404);
            return Mapear(pedido);
        }

        public async Task<List<PedidoDto>> TodosAsync(string? estado)
        {
            var q = Consulta();
            if (!string.IsNullOrWhiteSpace(estado)) q = q.Where(p => p.Estado == estado);
            var pedidos = await q.OrderByDescending(p => p.Fecha).ToListAsync();
            return pedidos.Select(Mapear).ToList();
        }

        public async Task<PedidoDto> CambiarEstadoAsync(int id, string estado)
        {
            var pedido = await CargarParaEditarAsync(id);
            AplicarEstado(pedido, estado);
            await _context.SaveChangesAsync();
            return await ObtenerAsync(id, 0, true);
        }

        public async Task<PedidoDto> CambiarEstadoPagoAsync(int id, string estado)
        {
            if (!EstadosPago.Editables.Contains(estado))
                throw new AppException("Estado de pago no válido.", 400);

            var pedido = await CargarParaEditarAsync(id);
            if (pedido.Pago == null)
                throw new AppException("El pedido no tiene un pago registrado.", 400);
            if (pedido.Estado == EstadosPedido.Cancelado)
                throw new AppException("El pedido está cancelado.", 400);

            pedido.Pago.Estado = estado;

            if (estado == EstadosPago.Aprobado && pedido.Estado == EstadosPedido.Pendiente)
                pedido.Estado = EstadosPedido.Pagado;
            else if (estado != EstadosPago.Aprobado && pedido.Estado == EstadosPedido.Pagado)
                pedido.Estado = EstadosPedido.Pendiente;

            await _context.SaveChangesAsync();
            return await ObtenerAsync(id, 0, true);
        }

        public async Task<PedidoDto> CancelarAsync(int id, int userId)
        {
            var pedido = await CargarParaEditarAsync(id);
            if (pedido.UsuarioId != userId)
                throw new AppException("Pedido no encontrado.", 404);
            if (pedido.Estado != EstadosPedido.Pendiente && pedido.Estado != EstadosPedido.Pagado)
                throw new AppException("Este pedido ya no se puede cancelar.", 400);

            AplicarEstado(pedido, EstadosPedido.Cancelado);
            await _context.SaveChangesAsync();
            return await ObtenerAsync(id, userId, true);
        }

        // ===== Auxiliares =====
        private static void AplicarEstado(Pedido pedido, string nuevo)
        {
            if (!EstadosPedido.Todos.Contains(nuevo))
                throw new AppException("Estado no válido.", 400);
            if (pedido.Estado == EstadosPedido.Cancelado)
                throw new AppException("El pedido ya está cancelado.", 400);
            if (pedido.Estado == EstadosPedido.Entregado)
                throw new AppException("El pedido ya fue entregado.", 400);

            if (nuevo == EstadosPedido.Cancelado)
            {
                foreach (var d in pedido.Detalles)
                    if (d.Producto != null) d.Producto.Stock += d.Cantidad;   // devuelve el stock

                if (pedido.Pago != null)
                    pedido.Pago.Estado = pedido.Pago.Estado == EstadosPago.Aprobado
                        ? EstadosPago.Reembolsar
                        : EstadosPago.Cancelado;
            }
            else if (nuevo == EstadosPedido.Pagado && pedido.Pago != null)
            {
                pedido.Pago.Estado = EstadosPago.Aprobado;
            }
            else if (nuevo == EstadosPedido.Entregado && pedido.Pago is { Metodo: "Contraentrega" })
            {
                pedido.Pago.Estado = EstadosPago.Aprobado;   // se cobró al entregar
            }

            pedido.Estado = nuevo;
        }

        private async Task<Pedido> CargarParaEditarAsync(int id) =>
            await _context.Pedido
                .Include(p => p.Detalles).ThenInclude(d => d.Producto)
                .Include(p => p.Pago)
                .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new AppException("Pedido no encontrado.", 404);

        private IQueryable<Pedido> Consulta() =>
            _context.Pedido.AsNoTracking()
                .Include(p => p.Usuario)
                .Include(p => p.Pago)
                .Include(p => p.Detalles).ThenInclude(d => d.Producto);

        private static PedidoDto Mapear(Pedido p) => new()
        {
            Id = p.Id,
            Fecha = p.Fecha,
            Estado = p.Estado,
            Subtotal = p.Subtotal,
            Impuestos = p.Impuestos,
            Total = p.Total,
            DireccionEnvio = p.DireccionEnvio,
            Ciudad = p.Ciudad,
            Telefono = p.Telefono,
            ClienteNombre = p.Usuario?.Nombre ?? "",
            ClienteEmail = p.Usuario?.Email ?? "",
            MetodoPago = p.Pago?.Metodo,
            EstadoPago = p.Pago?.Estado,
            Referencia = p.Pago?.Referencia,
            Items = p.Detalles.Select(d => new PedidoItemDto
            {
                ProductoId = d.ProductoId,
                Nombre = d.NombreProducto,
                ImagenUrl = d.Producto?.ImagenUrl,
                PrecioUnitario = d.PrecioUnitario,
                Cantidad = d.Cantidad,
                Subtotal = d.Subtotal
            }).ToList()
        };
    }
}