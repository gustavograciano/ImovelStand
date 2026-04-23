using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImovelStand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VendaWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormaPagamento",
                table: "Vendas");

            migrationBuilder.RenameColumn(
                name: "ValorVenda",
                table: "Vendas",
                newName: "ValorFinal");

            migrationBuilder.RenameColumn(
                name: "ValorEntrada",
                table: "Vendas",
                newName: "CondicaoFinal_ValorTotal");

            migrationBuilder.RenameColumn(
                name: "DataVenda",
                table: "Vendas",
                newName: "DataFechamento");

            // Converte strings de Venda.Status para códigos do enum StatusVenda antes do ALTER.
            migrationBuilder.Sql(@"
                UPDATE [Vendas] SET [Status] = CASE [Status]
                    WHEN N'Negociada' THEN '0'
                    WHEN N'EmContrato' THEN '1'
                    WHEN N'Concluída' THEN '2'
                    WHEN N'Concluida' THEN '2'
                    WHEN N'Assinada' THEN '2'
                    WHEN N'Cancelada' THEN '3'
                    WHEN N'Distratada' THEN '4'
                    ELSE '0'
                END;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Vendas",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Concluída");

            migrationBuilder.AlterColumn<string>(
                name: "Observacoes",
                table: "Vendas",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CondicaoFinal_ChavesDataPrevista",
                table: "Vendas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CondicaoFinal_Entrada",
                table: "Vendas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "CondicaoFinal_EntradaData",
                table: "Vendas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CondicaoFinal_Indice",
                table: "Vendas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CondicaoFinal_PrimeiraParcelaData",
                table: "Vendas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CondicaoFinal_QtdParcelasMensais",
                table: "Vendas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CondicaoFinal_QtdPosChaves",
                table: "Vendas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CondicaoFinal_QtdSemestrais",
                table: "Vendas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CondicaoFinal_Sinal",
                table: "Vendas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "CondicaoFinal_SinalData",
                table: "Vendas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CondicaoFinal_TaxaJurosAnual",
                table: "Vendas",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CondicaoFinal_ValorChaves",
                table: "Vendas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CondicaoFinal_ValorParcelaMensal",
                table: "Vendas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CondicaoFinal_ValorPosChaves",
                table: "Vendas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CondicaoFinal_ValorSemestral",
                table: "Vendas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ContratoUrl",
                table: "Vendas",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CorretorCaptacaoId",
                table: "Vendas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CorretorId",
                table: "Vendas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Vendas",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAprovacao",
                table: "Vendas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GerenteAprovadorId",
                table: "Vendas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Numero",
                table: "Vendas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PropostaId",
                table: "Vendas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Vendas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Creci",
                table: "Usuarios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PercentualComissao",
                table: "Usuarios",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoLoginEm",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Comissoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Percentual = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataAprovacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataPagamento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comissoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comissoes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comissoes_Vendas_VendaId",
                        column: x => x.VendaId,
                        principalTable: "Vendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Creci", "PercentualComissao", "UltimoLoginEm" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Creci", "PercentualComissao", "UltimoLoginEm" },
                values: new object[] { null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_CorretorCaptacaoId",
                table: "Vendas",
                column: "CorretorCaptacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_CorretorId",
                table: "Vendas",
                column: "CorretorId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_GerenteAprovadorId",
                table: "Vendas",
                column: "GerenteAprovadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_PropostaId",
                table: "Vendas",
                column: "PropostaId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_TenantId_Numero",
                table: "Vendas",
                columns: new[] { "TenantId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comissoes_UsuarioId",
                table: "Comissoes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Comissoes_VendaId_UsuarioId_Tipo",
                table: "Comissoes",
                columns: new[] { "VendaId", "UsuarioId", "Tipo" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Vendas_Propostas_PropostaId",
                table: "Vendas",
                column: "PropostaId",
                principalTable: "Propostas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Vendas_Usuarios_CorretorCaptacaoId",
                table: "Vendas",
                column: "CorretorCaptacaoId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Vendas_Usuarios_CorretorId",
                table: "Vendas",
                column: "CorretorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vendas_Usuarios_GerenteAprovadorId",
                table: "Vendas",
                column: "GerenteAprovadorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vendas_Propostas_PropostaId",
                table: "Vendas");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendas_Usuarios_CorretorCaptacaoId",
                table: "Vendas");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendas_Usuarios_CorretorId",
                table: "Vendas");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendas_Usuarios_GerenteAprovadorId",
                table: "Vendas");

            migrationBuilder.DropTable(
                name: "Comissoes");

            migrationBuilder.DropIndex(
                name: "IX_Vendas_CorretorCaptacaoId",
                table: "Vendas");

            migrationBuilder.DropIndex(
                name: "IX_Vendas_CorretorId",
                table: "Vendas");

            migrationBuilder.DropIndex(
                name: "IX_Vendas_GerenteAprovadorId",
                table: "Vendas");

            migrationBuilder.DropIndex(
                name: "IX_Vendas_PropostaId",
                table: "Vendas");

            migrationBuilder.DropIndex(
                name: "IX_Vendas_TenantId_Numero",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "CondicaoFinal_ChavesDataPrevista",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "CondicaoFinal_Entrada",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "CondicaoFinal_EntradaData",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "CondicaoFinal_Indice",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "CondicaoFinal_PrimeiraParcelaData",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "CondicaoFinal_QtdParcelasMensais",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "CondicaoFinal_QtdPosChaves",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "CondicaoFinal_QtdSemestrais",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "CondicaoFinal_Sinal",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "CondicaoFinal_SinalData",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "CondicaoFinal_TaxaJurosAnual",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "CondicaoFinal_ValorChaves",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "CondicaoFinal_ValorParcelaMensal",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "CondicaoFinal_ValorPosChaves",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "CondicaoFinal_ValorSemestral",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "ContratoUrl",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "CorretorCaptacaoId",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "CorretorId",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "DataAprovacao",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "GerenteAprovadorId",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "PropostaId",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "Creci",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "PercentualComissao",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "UltimoLoginEm",
                table: "Usuarios");

            migrationBuilder.RenameColumn(
                name: "ValorFinal",
                table: "Vendas",
                newName: "ValorVenda");

            migrationBuilder.RenameColumn(
                name: "DataFechamento",
                table: "Vendas",
                newName: "DataVenda");

            migrationBuilder.RenameColumn(
                name: "CondicaoFinal_ValorTotal",
                table: "Vendas",
                newName: "ValorEntrada");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Vendas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Concluída",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Observacoes",
                table: "Vendas",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormaPagamento",
                table: "Vendas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
