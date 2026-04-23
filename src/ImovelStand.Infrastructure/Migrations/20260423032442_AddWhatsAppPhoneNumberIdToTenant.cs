using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImovelStand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsAppPhoneNumberIdToTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WhatsAppPhoneNumberId",
                table: "Tenants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "WhatsAppPhoneNumberId",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhatsAppPhoneNumberId",
                table: "Tenants");
        }
    }
}
