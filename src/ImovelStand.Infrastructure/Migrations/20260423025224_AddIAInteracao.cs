using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImovelStand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIAInteracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IAInteracoes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    Operacao = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PromptVersao = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    InputTokens = table.Column<int>(type: "int", nullable: false),
                    OutputTokens = table.Column<int>(type: "int", nullable: false),
                    CustoUsd = table.Column<decimal>(type: "decimal(10,6)", nullable: false),
                    DuracaoMs = table.Column<int>(type: "int", nullable: false),
                    Sucesso = table.Column<bool>(type: "bit", nullable: false),
                    MensagemErro = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InputHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DoCache = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IAInteracoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IAInteracoes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IAInteracoes_InputHash",
                table: "IAInteracoes",
                column: "InputHash");

            migrationBuilder.CreateIndex(
                name: "IX_IAInteracoes_TenantId_CreatedAt",
                table: "IAInteracoes",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_IAInteracoes_TenantId_Operacao_CreatedAt",
                table: "IAInteracoes",
                columns: new[] { "TenantId", "Operacao", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_IAInteracoes_UsuarioId",
                table: "IAInteracoes",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IAInteracoes");
        }
    }
}
