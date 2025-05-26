using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Change_Status_ModifierGroups_And_ModifierOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ModifierOptions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ModifierGroups");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ModifierOptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ModifierGroups",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ModifierOptions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ModifierGroups");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ModifierOptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ModifierGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
