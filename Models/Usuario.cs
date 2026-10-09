namespace EcommerceAPI.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;   // vacío si la cuenta es solo de Google
        public string Rol { get; set; } = Roles.Cliente;
        public string? GoogleId { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string? FotoUrl { get; set; }
        public string? FotoPublicId { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
        public Carrito? Carrito { get; set; }
    }
}