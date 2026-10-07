using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace EcommerceAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterDatabase()
            //    .Annotation("MySQL:Charset", "utf8mb4");

           // migrationBuilder.CreateTable(
            //    name: "Producto",
             //   columns: table => new
              //  {
              //      Id = table.Column<int>(type: "int", nullable: false)
               //         .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
               //     Nombre = table.Column<string>(type: "longtext", nullable: false),
               //     Descripcion = table.Column<string>(type: "longtext", nullable: false),
               //     Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
              //      Stock = table.Column<int>(type: "int", nullable: false),
              //      ImagenUrl = table.Column<string>(type: "longtext", nullable: true),
               //     ImagenPublicId = table.Column<string>(type: "longtext", nullable: true)
               // },
              //  constraints: table =>
              //  {
              //      table.PrimaryKey("PK_Producto", x => x.Id);
              //  })
               // .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           // migrationBuilder.DropTable(
              //  name: "Producto");
        }
    }
}
