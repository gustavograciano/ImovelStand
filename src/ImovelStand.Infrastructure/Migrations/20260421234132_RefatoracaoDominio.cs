using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ImovelStand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefatoracaoDominio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Apartamentos_Numero",
                table: "Apartamentos");

            migrationBuilder.DropColumn(
                name: "AreaMetrosQuadrados",
                table: "Apartamentos");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Apartamentos");

            migrationBuilder.RenameColumn(
                name: "Quartos",
                table: "Apartamentos",
                newName: "TorreId");

            migrationBuilder.RenameColumn(
                name: "Preco",
                table: "Apartamentos",
                newName: "PrecoAtual");

            migrationBuilder.RenameColumn(
                name: "Banheiros",
                table: "Apartamentos",
                newName: "TipologiaId");

            migrationBuilder.RenameColumn(
                name: "Andar",
                table: "Apartamentos",
                newName: "Pavimento");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Apartamentos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Disponível");

            migrationBuilder.AlterColumn<string>(
                name: "Numero",
                table: "Apartamentos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Observacoes",
                table: "Apartamentos",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Orientacao",
                table: "Apartamentos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Empreendimentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(220)", maxLength: 220, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Construtora = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Endereco_Logradouro = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Endereco_Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Endereco_Complemento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Endereco_Bairro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Endereco_Cidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Endereco_Uf = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Endereco_Cep = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    DataLancamento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataEntregaPrevista = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    VgvEstimado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empreendimentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntidadeTipo = table.Column<int>(type: "int", nullable: false),
                    EntidadeId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Legenda = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fotos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoPrecos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApartamentoId = table.Column<int>(type: "int", nullable: false),
                    PrecoAnterior = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecoNovo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AlteradoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    DataAlteracao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoPrecos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricoPrecos_Apartamentos_ApartamentoId",
                        column: x => x.ApartamentoId,
                        principalTable: "Apartamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tipologias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpreendimentoId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AreaPrivativa = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    AreaTotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Quartos = table.Column<int>(type: "int", nullable: false),
                    Suites = table.Column<int>(type: "int", nullable: false),
                    Banheiros = table.Column<int>(type: "int", nullable: false),
                    Vagas = table.Column<int>(type: "int", nullable: false),
                    PrecoBase = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PlantaUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tipologias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tipologias_Empreendimentos_EmpreendimentoId",
                        column: x => x.EmpreendimentoId,
                        principalTable: "Empreendimentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Torres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpreendimentoId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Pavimentos = table.Column<int>(type: "int", nullable: false),
                    ApartamentosPorPavimento = table.Column<int>(type: "int", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Torres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Torres_Empreendimentos_EmpreendimentoId",
                        column: x => x.EmpreendimentoId,
                        principalTable: "Empreendimentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Numero", "Observacoes", "Orientacao", "PrecoAtual", "Status", "TorreId" },
                values: new object[] { "0101", null, 0, 350000.00m, 0, 1 });

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Numero", "Observacoes", "Orientacao", "PrecoAtual", "Status", "TorreId" },
                values: new object[] { "0102", null, 1, 520000.00m, 0, 1 });

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Numero", "Observacoes", "Orientacao", "PrecoAtual", "Status", "TorreId" },
                values: new object[] { "0201", null, 0, 353500.00m, 0, 1 });

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Numero", "Observacoes", "Orientacao", "PrecoAtual", "Status", "TorreId" },
                values: new object[] { "0202", null, 1, 525200.00m, 0, 1 });

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Numero", "Observacoes", "Orientacao", "PrecoAtual", "Status", "TipologiaId", "TorreId" },
                values: new object[] { "0301", null, 0, 357000.00m, 0, 1, 1 });

            migrationBuilder.InsertData(
                table: "Empreendimentos",
                columns: new[] { "Id", "Construtora", "DataCadastro", "DataEntregaPrevista", "DataLancamento", "Descricao", "Nome", "Slug", "Status", "VgvEstimado", "Endereco_Bairro", "Endereco_Cep", "Endereco_Cidade", "Endereco_Complemento", "Endereco_Logradouro", "Endereco_Numero", "Endereco_Uf" },
                values: new object[] { 1, "Construtora Demo Ltda", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2027, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Empreendimento demo para ambiente de desenvolvimento", "Residencial Exemplo", "residencial-exemplo", 1, 38000000m, "Centro", "01000-000", "Sao Paulo", null, "Avenida Demo", "1000", "SP" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "SenhaHash",
                value: "$2a$11$r4aHE2PnR4xi9noJxkzqe.2SIC5DqPZvinTi8EmFOHsMRIWcrPkqi");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "SenhaHash",
                value: "$2a$11$D8sg3FrM1EI689Z905iG2ubYw/m6LSlI3au9TWZFWd9dCFhw9rxQS");

            migrationBuilder.InsertData(
                table: "Tipologias",
                columns: new[] { "Id", "AreaPrivativa", "AreaTotal", "Banheiros", "DataCadastro", "EmpreendimentoId", "Nome", "PlantaUrl", "PrecoBase", "Quartos", "Suites", "Vagas" },
                values: new object[,]
                {
                    { 1, 55m, 70m, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "2Q Garden", null, 350000m, 2, 0, 1 },
                    { 2, 75m, 95m, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "3Q Suite", null, 520000m, 3, 1, 2 },
                    { 3, 140m, 180m, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Cobertura Duplex", null, 1200000m, 3, 2, 2 }
                });

            migrationBuilder.InsertData(
                table: "Torres",
                columns: new[] { "Id", "ApartamentosPorPavimento", "DataCadastro", "EmpreendimentoId", "Nome", "Pavimentos" },
                values: new object[,]
                {
                    { 1, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Torre A", 12 },
                    { 2, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Torre B", 12 }
                });

            migrationBuilder.InsertData(
                table: "Apartamentos",
                columns: new[] { "Id", "DataCadastro", "Numero", "Observacoes", "Orientacao", "Pavimento", "PrecoAtual", "TipologiaId", "TorreId" },
                values: new object[,]
                {
                    { 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0302", null, 1, 3, 530400.00m, 2, 1 },
                    { 7, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0401", null, 0, 4, 360500.00m, 1, 1 },
                    { 8, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0402", null, 1, 4, 535600.00m, 2, 1 },
                    { 9, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0501", null, 0, 5, 364000.00m, 1, 1 },
                    { 10, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0502", null, 1, 5, 540800.00m, 2, 1 },
                    { 11, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0601", null, 0, 6, 367500.00m, 1, 1 },
                    { 12, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0602", null, 1, 6, 546000.00m, 2, 1 },
                    { 13, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0701", null, 0, 7, 371000.00m, 1, 1 },
                    { 14, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0702", null, 1, 7, 551200.00m, 2, 1 },
                    { 15, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0801", null, 0, 8, 374500.00m, 1, 1 },
                    { 16, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0802", null, 1, 8, 556400.00m, 2, 1 },
                    { 17, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0901", null, 0, 9, 378000.00m, 1, 1 },
                    { 18, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0902", null, 1, 9, 561600.00m, 2, 1 },
                    { 19, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1001", null, 0, 10, 381500.00m, 1, 1 },
                    { 20, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1002", null, 1, 10, 566800.00m, 2, 1 },
                    { 21, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1101", null, 0, 11, 385000.00m, 1, 1 },
                    { 22, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1102", null, 1, 11, 572000.00m, 2, 1 },
                    { 23, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1201", null, 0, 12, 1332000.00m, 3, 1 },
                    { 24, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1202", null, 1, 12, 1332000.00m, 3, 1 },
                    { 25, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0101", null, 0, 1, 350000.00m, 1, 2 },
                    { 26, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0102", null, 1, 1, 520000.00m, 2, 2 },
                    { 27, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0201", null, 0, 2, 353500.00m, 1, 2 },
                    { 28, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0202", null, 1, 2, 525200.00m, 2, 2 },
                    { 29, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0301", null, 0, 3, 357000.00m, 1, 2 },
                    { 30, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0302", null, 1, 3, 530400.00m, 2, 2 },
                    { 31, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0401", null, 0, 4, 360500.00m, 1, 2 },
                    { 32, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0402", null, 1, 4, 535600.00m, 2, 2 },
                    { 33, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0501", null, 0, 5, 364000.00m, 1, 2 },
                    { 34, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0502", null, 1, 5, 540800.00m, 2, 2 },
                    { 35, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0601", null, 0, 6, 367500.00m, 1, 2 },
                    { 36, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0602", null, 1, 6, 546000.00m, 2, 2 },
                    { 37, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0701", null, 0, 7, 371000.00m, 1, 2 },
                    { 38, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0702", null, 1, 7, 551200.00m, 2, 2 },
                    { 39, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0801", null, 0, 8, 374500.00m, 1, 2 },
                    { 40, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0802", null, 1, 8, 556400.00m, 2, 2 },
                    { 41, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0901", null, 0, 9, 378000.00m, 1, 2 },
                    { 42, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0902", null, 1, 9, 561600.00m, 2, 2 },
                    { 43, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1001", null, 0, 10, 381500.00m, 1, 2 },
                    { 44, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1002", null, 1, 10, 566800.00m, 2, 2 },
                    { 45, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1101", null, 0, 11, 385000.00m, 1, 2 },
                    { 46, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1102", null, 1, 11, 572000.00m, 2, 2 },
                    { 47, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1201", null, 0, 12, 1332000.00m, 3, 2 },
                    { 48, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1202", null, 1, 12, 1332000.00m, 3, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Apartamentos_TipologiaId",
                table: "Apartamentos",
                column: "TipologiaId");

            migrationBuilder.CreateIndex(
                name: "IX_Apartamentos_TorreId_Numero",
                table: "Apartamentos",
                columns: new[] { "TorreId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empreendimentos_Slug",
                table: "Empreendimentos",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fotos_EntidadeTipo_EntidadeId",
                table: "Fotos",
                columns: new[] { "EntidadeTipo", "EntidadeId" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoPrecos_ApartamentoId",
                table: "HistoricoPrecos",
                column: "ApartamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tipologias_EmpreendimentoId_Nome",
                table: "Tipologias",
                columns: new[] { "EmpreendimentoId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Torres_EmpreendimentoId_Nome",
                table: "Torres",
                columns: new[] { "EmpreendimentoId", "Nome" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Apartamentos_Tipologias_TipologiaId",
                table: "Apartamentos",
                column: "TipologiaId",
                principalTable: "Tipologias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Apartamentos_Torres_TorreId",
                table: "Apartamentos",
                column: "TorreId",
                principalTable: "Torres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Apartamentos_Tipologias_TipologiaId",
                table: "Apartamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Apartamentos_Torres_TorreId",
                table: "Apartamentos");

            migrationBuilder.DropTable(
                name: "Fotos");

            migrationBuilder.DropTable(
                name: "HistoricoPrecos");

            migrationBuilder.DropTable(
                name: "Tipologias");

            migrationBuilder.DropTable(
                name: "Torres");

            migrationBuilder.DropTable(
                name: "Empreendimentos");

            migrationBuilder.DropIndex(
                name: "IX_Apartamentos_TipologiaId",
                table: "Apartamentos");

            migrationBuilder.DropIndex(
                name: "IX_Apartamentos_TorreId_Numero",
                table: "Apartamentos");

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DropColumn(
                name: "Observacoes",
                table: "Apartamentos");

            migrationBuilder.DropColumn(
                name: "Orientacao",
                table: "Apartamentos");

            migrationBuilder.RenameColumn(
                name: "TorreId",
                table: "Apartamentos",
                newName: "Quartos");

            migrationBuilder.RenameColumn(
                name: "TipologiaId",
                table: "Apartamentos",
                newName: "Banheiros");

            migrationBuilder.RenameColumn(
                name: "PrecoAtual",
                table: "Apartamentos",
                newName: "Preco");

            migrationBuilder.RenameColumn(
                name: "Pavimento",
                table: "Apartamentos",
                newName: "Andar");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Apartamentos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Disponível",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Numero",
                table: "Apartamentos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<decimal>(
                name: "AreaMetrosQuadrados",
                table: "Apartamentos",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Apartamentos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AreaMetrosQuadrados", "Descricao", "Numero", "Preco", "Quartos", "Status" },
                values: new object[] { 65.5m, "Apartamento com 2 quartos, sala, cozinha e área de serviço", "101", 250000.00m, 2, "Disponível" });

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AreaMetrosQuadrados", "Descricao", "Numero", "Preco", "Quartos", "Status" },
                values: new object[] { 85.0m, "Apartamento com 3 quartos sendo 1 suíte, sala ampla e varanda", "102", 350000.00m, 3, "Disponível" });

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AreaMetrosQuadrados", "Descricao", "Numero", "Preco", "Quartos", "Status" },
                values: new object[] { 65.5m, "Apartamento com 2 quartos, sala, cozinha e área de serviço", "201", 260000.00m, 2, "Disponível" });

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AreaMetrosQuadrados", "Descricao", "Numero", "Preco", "Quartos", "Status" },
                values: new object[] { 85.0m, "Apartamento com 3 quartos sendo 1 suíte, sala ampla e varanda", "202", 360000.00m, 3, "Disponível" });

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AreaMetrosQuadrados", "Banheiros", "Descricao", "Numero", "Preco", "Quartos", "Status" },
                values: new object[] { 120.0m, 3, "Cobertura com 4 quartos sendo 2 suítes, sala de estar e jantar, varanda gourmet", "301", 480000.00m, 4, "Disponível" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "SenhaHash",
                value: "$2a$11$8vJ5xZxZxZxZxZxZxZxZxuK9H9H9H9H9H9H9H9H9H9H9H9H9H9H9H");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "SenhaHash",
                value: "$2a$11$7wI6yYyYyYyYyYyYyYyYyuJ8G8G8G8G8G8G8G8G8G8G8G8G8G8G8G");

            migrationBuilder.CreateIndex(
                name: "IX_Apartamentos_Numero",
                table: "Apartamentos",
                column: "Numero",
                unique: true);
        }
    }
}
