using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImovelStand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPrecificacaoDinamica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrecosMercado",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cidade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Uf = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Bairro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quartos = table.Column<int>(type: "int", nullable: false),
                    PrecoMedioM2 = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    DesvioPadraoM2 = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    QtdAmostras = table.Column<int>(type: "int", nullable: false),
                    Fonte = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DataReferencia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrecosMercado", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SugestoesPreco",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApartamentoId = table.Column<int>(type: "int", nullable: false),
                    PrecoAtual = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    PrecoSugerido = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    VariacaoPct = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Justificativa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    VelocidadeVendaSemanal = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    VelocidadeMercado = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    Confianca = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AceitaPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    RespondidaEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoRejeicao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SugestoesPreco", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SugestoesPreco_Apartamentos_ApartamentoId",
                        column: x => x.ApartamentoId,
                        principalTable: "Apartamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SugestoesPreco_Usuarios_AceitaPorUsuarioId",
                        column: x => x.AceitaPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrecosMercado_Cidade_Uf_Bairro_Quartos_DataReferencia",
                table: "PrecosMercado",
                columns: new[] { "Cidade", "Uf", "Bairro", "Quartos", "DataReferencia" });

            migrationBuilder.CreateIndex(
                name: "IX_SugestoesPreco_AceitaPorUsuarioId",
                table: "SugestoesPreco",
                column: "AceitaPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_SugestoesPreco_ApartamentoId",
                table: "SugestoesPreco",
                column: "ApartamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_SugestoesPreco_TenantId_ApartamentoId",
                table: "SugestoesPreco",
                columns: new[] { "TenantId", "ApartamentoId" });

            migrationBuilder.CreateIndex(
                name: "IX_SugestoesPreco_TenantId_Status_CreatedAt",
                table: "SugestoesPreco",
                columns: new[] { "TenantId", "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrecosMercado");

            migrationBuilder.DropTable(
                name: "SugestoesPreco");
        }
    }
}
