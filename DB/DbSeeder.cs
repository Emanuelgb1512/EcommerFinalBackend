using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.DB
{
    public static class DbSeeder
    {
        private static readonly (string Nombre, string Descripcion)[] Categorias =
        {
            ("Fútbol", "Balones, guayos, canilleras, guantes de arquero y uniformes"),
            ("Baloncesto", "Balones, aros, zapatillas y ropa de juego"),
            ("Voleibol", "Balones, redes, rodilleras y calzado especializado"),
            ("Tenis y Pádel", "Raquetas, pelotas, grips y accesorios"),
            ("Running y Atletismo", "Calzado, ropa técnica y accesorios para correr"),
            ("Ciclismo", "Bicicletas, cascos, ropa y repuestos"),
            ("Natación", "Trajes de baño, gafas, gorros y accesorios"),
            ("Gimnasio y Fitness", "Pesas, mancuernas, bandas elásticas y máquinas"),
            ("Yoga y Pilates", "Tapetes, bloques, pelotas y ropa cómoda"),
            ("Boxeo y Artes Marciales", "Guantes, protectores, sacos y uniformes"),
            ("Béisbol y Softbol", "Guantes, bates, pelotas y cascos"),
            ("Patinaje y Skateboard", "Patines, patinetas y protecciones"),
            ("Montaña y Camping", "Carpas, morrales, linternas y equipo outdoor"),
            ("Golf", "Palos, bolas, guantes y accesorios"),
            ("Calzado deportivo", "Zapatillas y tenis para todo tipo de deporte"),
            ("Ropa deportiva", "Camisetas, shorts, sudaderas, licras y chaquetas"),
            ("Accesorios deportivos", "Gorras, medias, muñequeras y guantes"),
            ("Hidratación y Termos", "Botellas, termos y cantimploras"),
            ("Suplementos y Nutrición", "Proteínas, barras energéticas y suplementos"),
            ("Protección y Recuperación", "Rodilleras, tobilleras, vendas y masajeadores"),
            ("Maletas y Bolsos", "Maletas, morrales y canguros deportivos"),
            ("Tecnología deportiva", "Relojes inteligentes, bandas de pulso y audífonos")
        };

        public static async Task SeedCategoriasAsync(AppDbContext context)
        {
            if (await context.Categoria.AnyAsync()) return;

            context.Categoria.AddRange(Categorias.Select(c =>
                new Categoria { Nombre = c.Nombre, Descripcion = c.Descripcion }));

            await context.SaveChangesAsync();
        }
    }
}