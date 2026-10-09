using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models.Dtos
{
    // ===== Auth y usuarios =====
    public class RegisterDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre es demasiado largo.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener mínimo 6 caracteres.")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; } = string.Empty;
    }

    public class GoogleLoginDto
    {
        [Required(ErrorMessage = "Falta la credencial de Google.")]
        public string Credential { get; set; } = string.Empty;
    }

    public class UsuarioInfoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string? FotoUrl { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UsuarioInfoDto Usuario { get; set; } = new();
    }

    public class PerfilDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        [StringLength(30)] public string? Telefono { get; set; }
        [StringLength(200)] public string? Direccion { get; set; }
        [StringLength(100)] public string? Ciudad { get; set; }
    }

    public class CambiarPasswordDto
    {
        public string? PasswordActual { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener mínimo 6 caracteres.")]
        public string PasswordNueva { get; set; } = string.Empty;
    }

    public class CambiarRolDto
    {
        [Required] public string Rol { get; set; } = string.Empty;
    }

    public class CambiarActivoDto
    {
        public bool Activo { get; set; }
    }
    public class FotoPerfilDto
    {
        public string? ImagenBase64 { get; set; }
    }

    // ===== Categorías =====
    public class CategoriaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int CantidadProductos { get; set; }
    }

    public class CategoriaGuardarDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        [StringLength(300)] public string? Descripcion { get; set; }
    }

    // ===== Productos =====
    public class ProductoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? ImagenUrl { get; set; }
        public bool Activo { get; set; }
        public int? CategoriaId { get; set; }
        public string? CategoriaNombre { get; set; }
    }

    public class ProductoGuardarDto
    {
        [Required(ErrorMessage = "El nombre no puede estar vacío.")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Descripcion { get; set; }

        [Range(0, 1000000000, ErrorMessage = "El precio no puede ser negativo.")]
        public decimal Precio { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

        public int? CategoriaId { get; set; }
        public bool Activo { get; set; } = true;
        public string? ImagenBase64 { get; set; }   // opcional: solo si hay imagen nueva
    }

    public class ProductoFiltroDto
    {
        public string? Busqueda { get; set; }
        public int? CategoriaId { get; set; }
        public decimal? PrecioMin { get; set; }
        public decimal? PrecioMax { get; set; }
        public string? Orden { get; set; }          // precio_asc, precio_desc, nombre, recientes
        public int Pagina { get; set; } = 1;
        public int TamanoPagina { get; set; } = 12;
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public int TotalPaginas => TamanoPagina == 0 ? 0 : (int)Math.Ceiling((double)Total / TamanoPagina);
    }

    // ===== Carrito =====
    public class CarritoItemDto
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public int Stock { get; set; }
        public bool Activo { get; set; }
        public decimal Subtotal => PrecioUnitario * Cantidad;
    }

    public class CarritoDto
    {
        public List<CarritoItemDto> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.Subtotal);
        public int CantidadTotal => Items.Sum(i => i.Cantidad);
    }

    public class AgregarCarritoDto
    {
        public int ProductoId { get; set; }
        [Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100.")]
        public int Cantidad { get; set; } = 1;
    }

    public class CantidadDto
    {
        [Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100.")]
        public int Cantidad { get; set; }
    }

    // ===== Pedidos =====
    public class CheckoutDto
    {
        [Required(ErrorMessage = "La dirección de envío es obligatoria.")]
        [StringLength(200)]
        public string DireccionEnvio { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ciudad es obligatoria.")]
        [StringLength(100)]
        public string Ciudad { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [StringLength(30)]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona un método de pago.")]
        public string MetodoPago { get; set; } = "Tarjeta";   // Tarjeta, PSE, Contraentrega
    }

    public class PedidoItemDto
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class PedidoDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public string DireccionEnvio { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;
        public string? MetodoPago { get; set; }
        public string? EstadoPago { get; set; }
        public string? Referencia { get; set; }
        public List<PedidoItemDto> Items { get; set; } = new();
    }

    public class CambiarEstadoDto
    {
        [Required] public string Estado { get; set; } = string.Empty;
    }
    public class CambiarEstadoPagoDto
    {
        [Required] public string Estado { get; set; } = string.Empty;
    }
}