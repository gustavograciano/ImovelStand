using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImovelStand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TemplateNotificacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TemplatesNotificacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Canal = table.Column<int>(type: "int", nullable: false),
                    Assunto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Corpo = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplatesNotificacao", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemplatesNotificacao_TenantId_Codigo_Canal",
                table: "TemplatesNotificacao",
                columns: new[] { "TenantId", "Codigo", "Canal" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemplatesNotificacao");
        }
    }
}
