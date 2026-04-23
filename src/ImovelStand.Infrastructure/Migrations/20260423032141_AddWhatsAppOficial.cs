using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImovelStand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsAppOficial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WhatsAppTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Idioma = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Corpo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    QtdVariaveis = table.Column<int>(type: "int", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsAppTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WhatsAppMensagens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    TelefoneContato = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NumeroEmpresa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Direcao = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TemplateId = table.Column<int>(type: "int", nullable: true),
                    VariaveisJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Conteudo = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    MediaUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProviderMessageId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MensagemErro = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    EnviadaEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EntregueEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LidaEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsAppMensagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhatsAppMensagens_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WhatsAppMensagens_WhatsAppTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "WhatsAppTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMensagens_ClienteId",
                table: "WhatsAppMensagens",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMensagens_ProviderMessageId",
                table: "WhatsAppMensagens",
                column: "ProviderMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMensagens_TemplateId",
                table: "WhatsAppMensagens",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMensagens_TenantId_ClienteId_CreatedAt",
                table: "WhatsAppMensagens",
                columns: new[] { "TenantId", "ClienteId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppTemplates_TenantId_Nome_Idioma",
                table: "WhatsAppTemplates",
                columns: new[] { "TenantId", "Nome", "Idioma" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WhatsAppMensagens");

            migrationBuilder.DropTable(
                name: "WhatsAppTemplates");
        }
    }
}
