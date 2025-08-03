using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Remove_UnitOfMeasure_And_LastCountedAt_Of_InventoryStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastCountedAt",
                table: "InventoryStock");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasure",
                table: "InventoryStock");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastCountedAt",
                table: "InventoryStock",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasure",
                table: "InventoryStock",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
