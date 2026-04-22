using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImovelStand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CrmClienteCompleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConjugeId",
                table: "Clientes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ConsentimentoLgpd",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsentimentoLgpdEm",
                table: "Clientes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CorretorResponsavelId",
                table: "Clientes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataNascimento",
                table: "Clientes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Empresa",
                table: "Clientes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Endereco_Bairro",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Endereco_Cep",
                table: "Clientes",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Endereco_Cidade",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Endereco_Complemento",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Endereco_Logradouro",
                table: "Clientes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Endereco_Numero",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Endereco_Uf",
                table: "Clientes",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstadoCivil",
                table: "Clientes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrigemLead",
                table: "Clientes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profissao",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegimeBens",
                table: "Clientes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RendaMensal",
                table: "Clientes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rg",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusFunil",
                table: "Clientes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Whatsapp",
                table: "Clientes",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClienteDependentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Cpf = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    DataNascimento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Parentesco = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClienteDependentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClienteDependentes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoInteracoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoInteracoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricoInteracoes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistoricoInteracoes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Visitas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    CorretorId = table.Column<int>(type: "int", nullable: false),
                    EmpreendimentoId = table.Column<int>(type: "int", nullable: false),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DuracaoMinutos = table.Column<int>(type: "int", nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GerouProposta = table.Column<bool>(type: "bit", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visitas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visitas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visitas_Empreendimentos_EmpreendimentoId",
                        column: x => x.EmpreendimentoId,
                        principalTable: "Empreendimentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visitas_Usuarios_CorretorId",
                        column: x => x.CorretorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_ConjugeId",
                table: "Clientes",
                column: "ConjugeId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_CorretorResponsavelId",
                table: "Clientes",
                column: "CorretorResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_ClienteDependentes_ClienteId",
                table: "ClienteDependentes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoInteracoes_ClienteId_DataHora",
                table: "HistoricoInteracoes",
                columns: new[] { "ClienteId", "DataHora" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoInteracoes_UsuarioId",
                table: "HistoricoInteracoes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_ClienteId",
                table: "Visitas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_CorretorId",
                table: "Visitas",
                column: "CorretorId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_EmpreendimentoId_DataHora",
                table: "Visitas",
                columns: new[] { "EmpreendimentoId", "DataHora" });

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Clientes_ConjugeId",
                table: "Clientes",
                column: "ConjugeId",
                principalTable: "Clientes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Usuarios_CorretorResponsavelId",
                table: "Clientes",
                column: "CorretorResponsavelId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Clientes_ConjugeId",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Usuarios_CorretorResponsavelId",
                table: "Clientes");

            migrationBuilder.DropTable(
                name: "ClienteDependentes");

            migrationBuilder.DropTable(
                name: "HistoricoInteracoes");

            migrationBuilder.DropTable(
                name: "Visitas");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_ConjugeId",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_CorretorResponsavelId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "ConjugeId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "ConsentimentoLgpd",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "ConsentimentoLgpdEm",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "CorretorResponsavelId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "DataNascimento",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Empresa",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Endereco_Bairro",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Endereco_Cep",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Endereco_Cidade",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Endereco_Complemento",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Endereco_Logradouro",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Endereco_Numero",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Endereco_Uf",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EstadoCivil",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "OrigemLead",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Profissao",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "RegimeBens",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "RendaMensal",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Rg",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "StatusFunil",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Whatsapp",
                table: "Clientes");
        }
    }
}
