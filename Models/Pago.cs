namespace EcommerceAPI.Models
{
    public class Pago
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public Pedido? Pedido { get; set; }
        public string Metodo { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Estado { get; set; } = "Pendiente";  
        public string Referencia { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}