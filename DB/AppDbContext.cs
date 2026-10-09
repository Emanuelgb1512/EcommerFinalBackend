using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace EcommerceAPI.DB
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Categoria> Categoria { get; set; }
        public DbSet<Producto> Producto { get; set; }
        public DbSet<Carrito> Carrito { get; set; }
        public DbSet<DetalleCarrito> DetalleCarrito { get; set; }
        public DbSet<Pedido> Pedido { get; set; }
        public DbSet<DetallePedido> DetallePedido { get; set; }
        public DbSet<Pago> Pago { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Usuario>(e =>
            {
                e.Property(u => u.Email).HasMaxLength(255);
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Rol).HasMaxLength(20);
                e.Property(u => u.GoogleId).HasMaxLength(100);
            });

            mb.Entity<Categoria>(e =>
            {
                e.Property(c => c.Nombre).HasMaxLength(100);
                e.HasIndex(c => c.Nombre).IsUnique();
            });

            mb.Entity<Producto>(e =>
            {
                e.Property(p => p.Precio).HasColumnType("decimal(18,2)");
                e.HasOne(p => p.Categoria).WithMany(c => c.Productos)
                    .HasForeignKey(p => p.CategoriaId).OnDelete(DeleteBehavior.SetNull);
                e.HasOne(p => p.Usuario).WithMany(u => u.Productos)
                    .HasForeignKey(p => p.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            });

            mb.Entity<Carrito>(e =>
            {
                e.HasOne(c => c.Usuario).WithOne(u => u.Carrito)
                    .HasForeignKey<Carrito>(c => c.UsuarioId);
            });

            mb.Entity<DetalleCarrito>(e =>
            {
                e.HasIndex(d => new { d.CarritoId, d.ProductoId }).IsUnique();
                e.HasOne(d => d.Carrito).WithMany(c => c.Detalles).HasForeignKey(d => d.CarritoId);
                e.HasOne(d => d.Producto).WithMany().HasForeignKey(d => d.ProductoId);
            });

            mb.Entity<Pedido>(e =>
            {
                e.Property(p => p.Subtotal).HasColumnType("decimal(18,2)");
                e.Property(p => p.Impuestos).HasColumnType("decimal(18,2)");
                e.Property(p => p.Total).HasColumnType("decimal(18,2)");
                e.Property(p => p.Estado).HasMaxLength(20);
                e.HasOne(p => p.Usuario).WithMany(u => u.Pedidos)
                    .HasForeignKey(p => p.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            });

            mb.Entity<DetallePedido>(e =>
            {
                e.Property(d => d.PrecioUnitario).HasColumnType("decimal(18,2)");
                e.Property(d => d.Subtotal).HasColumnType("decimal(18,2)");
                e.HasOne(d => d.Pedido).WithMany(p => p.Detalles).HasForeignKey(d => d.PedidoId);
                e.HasOne(d => d.Producto).WithMany()
                    .HasForeignKey(d => d.ProductoId).OnDelete(DeleteBehavior.Restrict);
            });

            mb.Entity<Pago>(e =>
            {
                e.Property(p => p.Monto).HasColumnType("decimal(18,2)");
                e.HasOne(p => p.Pedido).WithOne(p => p.Pago).HasForeignKey<Pago>(p => p.PedidoId);
            });

            mb.Entity<Categoria>().ToTable("categoria");
            mb.Entity<Producto>().ToTable("producto");
            mb.Entity<Usuario>().ToTable("usuario");
            mb.Entity<Carrito>().ToTable("carrito");
            mb.Entity<DetalleCarrito>().ToTable("detallecarrito");
            mb.Entity<Pedido>().ToTable("pedido");
            mb.Entity<DetallePedido>().ToTable("detallepedido");
            mb.Entity<Pago>().ToTable("pago");
        }
    }
}