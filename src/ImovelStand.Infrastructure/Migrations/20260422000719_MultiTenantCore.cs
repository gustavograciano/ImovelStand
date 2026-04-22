using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ImovelStand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MultiTenantCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Empreendimentos_Slug",
                table: "Empreendimentos");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Cpf",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Email",
                table: "Clientes");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Vendas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Usuarios",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Torres",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Tipologias",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Reservas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "HistoricoPrecos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Fotos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Empreendimentos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Clientes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Apartamentos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Planos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PrecoMensal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxEmpreendimentos = table.Column<int>(type: "int", nullable: false),
                    MaxUnidades = table.Column<int>(type: "int", nullable: false),
                    MaxUsuarios = table.Column<int>(type: "int", nullable: false),
                    FeaturesJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Cnpj = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PlanoId = table.Column<int>(type: "int", nullable: false),
                    TrialAte = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AtivoAte = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tenants_Planos_PlanoId",
                        column: x => x.PlanoId,
                        principalTable: "Planos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 2,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 3,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 4,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 5,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 6,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 7,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 8,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 9,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 10,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 11,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 12,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 13,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 14,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 15,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 16,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 17,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 18,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 19,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 20,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 21,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 22,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 23,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 24,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 25,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 26,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 27,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 28,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 29,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 30,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 31,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 32,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 33,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 34,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 35,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 36,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 37,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 38,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 39,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 40,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 41,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 42,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 43,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 44,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 45,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 46,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 47,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Apartamentos",
                keyColumn: "Id",
                keyValue: 48,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Empreendimentos",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.InsertData(
                table: "Planos",
                columns: new[] { "Id", "Ativo", "FeaturesJson", "MaxEmpreendimentos", "MaxUnidades", "MaxUsuarios", "Nome", "PrecoMensal" },
                values: new object[,]
                {
                    { 1, true, null, 1, 100, 3, "Starter", 299m },
                    { 2, true, null, 5, 500, 15, "Pro", 899m },
                    { 3, true, null, 50, 10000, 100, "Business", 2499m }
                });

            migrationBuilder.UpdateData(
                table: "Tipologias",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Tipologias",
                keyColumn: "Id",
                keyValue: 2,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Tipologias",
                keyColumn: "Id",
                keyValue: 3,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Torres",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Torres",
                keyColumn: "Id",
                keyValue: 2,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "TenantId",
                value: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "Ativo", "AtivoAte", "Cnpj", "CreatedAt", "Nome", "PlanoId", "Slug", "TrialAte" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), true, null, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Imobiliária Demo", 2, "demo", null });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TenantId_Email",
                table: "Usuarios",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empreendimentos_TenantId_Slug",
                table: "Empreendimentos",
                columns: new[] { "TenantId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_TenantId_Cpf",
                table: "Clientes",
                columns: new[] { "TenantId", "Cpf" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_TenantId_Email",
                table: "Clientes",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Cnpj",
                table: "Tenants",
                column: "Cnpj",
                unique: true,
                filter: "[Cnpj] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_PlanoId",
                table: "Tenants",
                column: "PlanoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Slug",
                table: "Tenants",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "Planos");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_TenantId_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Empreendimentos_TenantId_Slug",
                table: "Empreendimentos");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_TenantId_Cpf",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_TenantId_Email",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Torres");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Tipologias");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "HistoricoPrecos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Fotos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Empreendimentos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Apartamentos");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empreendimentos_Slug",
                table: "Empreendimentos",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Cpf",
                table: "Clientes",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Email",
                table: "Clientes",
                column: "Email",
                unique: true);
        }
    }
}
