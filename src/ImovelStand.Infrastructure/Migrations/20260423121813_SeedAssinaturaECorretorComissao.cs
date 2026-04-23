using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImovelStand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAssinaturaECorretorComissao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Assinaturas",
                columns: new[] { "Id", "CanceladaEm", "CreatedAt", "DataInicio", "IuguCustomerId", "IuguSubscriptionId", "MotivoCancelamento", "PlanoId", "ProximaCobranca", "Status", "TenantId", "TrialFimEm", "UpdatedAt" },
                values: new object[] { 1, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed-demo-cust", "seed-demo-sub", null, 2, new DateTime(2025, 1, 31, 0, 0, 0, 0, DateTimeKind.Utc), 1, new Guid("11111111-1111-1111-1111-111111111111"), null, null });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Creci", "PercentualComissao" },
                values: new object[] { "SP-123456", 0.03m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Assinaturas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Creci", "PercentualComissao" },
                values: new object[] { null, null });
        }
    }
}
