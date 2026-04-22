using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImovelStand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PropostaSprint6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Propostas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ApartamentoId = table.Column<int>(type: "int", nullable: false),
                    CorretorId = table.Column<int>(type: "int", nullable: false),
                    Versao = table.Column<int>(type: "int", nullable: false),
                    PropostaOriginalId = table.Column<int>(type: "int", nullable: true),
                    ValorOferecido = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataValidade = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataRespostaCliente = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Condicao_ValorTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Condicao_Entrada = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Condicao_EntradaData = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Condicao_Sinal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Condicao_SinalData = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Condicao_QtdParcelasMensais = table.Column<int>(type: "int", nullable: false),
                    Condicao_ValorParcelaMensal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Condicao_PrimeiraParcelaData = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Condicao_QtdSemestrais = table.Column<int>(type: "int", nullable: false),
                    Condicao_ValorSemestral = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Condicao_ValorChaves = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Condicao_ChavesDataPrevista = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Condicao_QtdPosChaves = table.Column<int>(type: "int", nullable: false),
                    Condicao_ValorPosChaves = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Condicao_Indice = table.Column<int>(type: "int", nullable: false),
                    Condicao_TaxaJurosAnual = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Propostas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Propostas_Apartamentos_ApartamentoId",
                        column: x => x.ApartamentoId,
                        principalTable: "Apartamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Propostas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Propostas_Propostas_PropostaOriginalId",
                        column: x => x.PropostaOriginalId,
                        principalTable: "Propostas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Propostas_Usuarios_CorretorId",
                        column: x => x.CorretorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PropostaHistoricos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PropostaId = table.Column<int>(type: "int", nullable: false),
                    StatusAnterior = table.Column<int>(type: "int", nullable: false),
                    StatusNovo = table.Column<int>(type: "int", nullable: false),
                    AlteradoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataAlteracao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropostaHistoricos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropostaHistoricos_Propostas_PropostaId",
                        column: x => x.PropostaId,
                        principalTable: "Propostas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropostaHistoricos_PropostaId_DataAlteracao",
                table: "PropostaHistoricos",
                columns: new[] { "PropostaId", "DataAlteracao" });

            migrationBuilder.CreateIndex(
                name: "IX_Propostas_ApartamentoId",
                table: "Propostas",
                column: "ApartamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Propostas_ClienteId",
                table: "Propostas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Propostas_CorretorId",
                table: "Propostas",
                column: "CorretorId");

            migrationBuilder.CreateIndex(
                name: "IX_Propostas_PropostaOriginalId",
                table: "Propostas",
                column: "PropostaOriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_Propostas_TenantId_Numero",
                table: "Propostas",
                columns: new[] { "TenantId", "Numero" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropostaHistoricos");

            migrationBuilder.DropTable(
                name: "Propostas");
        }
    }
}
