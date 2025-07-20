using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.MenuCombo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Remove_ValidRange_And_Effective_Range_Of_BrandMenu_And_StoreMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EffectiveAt",
                table: "StoreMenuItemAvailability");

            migrationBuilder.DropColumn(
                name: "EffectiveEnd",
                table: "StoreMenuItemAvailability");

            migrationBuilder.DropColumn(
                name: "EffectiveAt",
                table: "StoreMenuAssignments");

            migrationBuilder.DropColumn(
                name: "EffectiveEnd",
                table: "StoreMenuAssignments");

            migrationBuilder.DropColumn(
                name: "ValidFrom",
                table: "Menu");

            migrationBuilder.DropColumn(
                name: "ValidTo",
                table: "Menu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveAt",
                table: "StoreMenuItemAvailability",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveEnd",
                table: "StoreMenuItemAvailability",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveAt",
                table: "StoreMenuAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveEnd",
                table: "StoreMenuAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidFrom",
                table: "Menu",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidTo",
                table: "Menu",
                type: "datetime2",
                nullable: true);
        }
    }
}
