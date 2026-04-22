using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImovelStand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PreferenciaContato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AceitaEmail",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AceitaSms",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AceitaWhatsapp",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AceitaEmail",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "AceitaSms",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "AceitaWhatsapp",
                table: "Clientes");
        }
    }
}
