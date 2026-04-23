using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImovelStand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnaliseCredito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitacoesAnaliseCredito",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    SolicitadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Provedor = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProviderItemId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RendaMediaComprovada = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    VolatilidadeRenda = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    DividasRecorrentes = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    CapacidadePagamento = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: true),
                    AlertasJson = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConsentimentoLgpd = table.Column<bool>(type: "bit", nullable: false),
                    ConsentimentoLgpdEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiraEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConcluidaEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MensagemErro = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitacoesAnaliseCredito", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitacoesAnaliseCredito_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitacoesAnaliseCredito_Usuarios_SolicitadoPorUsuarioId",
                        column: x => x.SolicitadoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesAnaliseCredito_ClienteId",
                table: "SolicitacoesAnaliseCredito",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesAnaliseCredito_ExpiraEm",
                table: "SolicitacoesAnaliseCredito",
                column: "ExpiraEm");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesAnaliseCredito_SolicitadoPorUsuarioId",
                table: "SolicitacoesAnaliseCredito",
                column: "SolicitadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesAnaliseCredito_TenantId_ClienteId",
                table: "SolicitacoesAnaliseCredito",
                columns: new[] { "TenantId", "ClienteId" });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesAnaliseCredito_Token",
                table: "SolicitacoesAnaliseCredito",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitacoesAnaliseCredito");
        }
    }
}
