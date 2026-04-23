using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImovelStand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNumeroWhatsApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NumerosWhatsApp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhoneNumberId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroExibicao = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Apelido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    OrdemRoundRobin = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumerosWhatsApp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NumerosWhatsApp_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NumerosWhatsApp_PhoneNumberId",
                table: "NumerosWhatsApp",
                column: "PhoneNumberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NumerosWhatsApp_TenantId_UsuarioId",
                table: "NumerosWhatsApp",
                columns: new[] { "TenantId", "UsuarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_NumerosWhatsApp_UsuarioId",
                table: "NumerosWhatsApp",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NumerosWhatsApp");
        }
    }
}
