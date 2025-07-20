using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.MenuCombo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Relationship_Of_BrandMenu_And_StoreMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionItems");

            migrationBuilder.DropTable(
                name: "Collections");

            migrationBuilder.CreateIndex(
                name: "IX_StoreMenuItemAvailability_BrandMenuItemId",
                table: "StoreMenuItemAvailability",
                column: "BrandMenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreMenuAssignments_BrandMenuId",
                table: "StoreMenuAssignments",
                column: "BrandMenuId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreMenuAssignments_Menu_BrandMenuId",
                table: "StoreMenuAssignments",
                column: "BrandMenuId",
                principalTable: "Menu",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StoreMenuItemAvailability_MenuItems_BrandMenuItemId",
                table: "StoreMenuItemAvailability",
                column: "BrandMenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreMenuAssignments_Menu_BrandMenuId",
                table: "StoreMenuAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_StoreMenuItemAvailability_MenuItems_BrandMenuItemId",
                table: "StoreMenuItemAvailability");

            migrationBuilder.DropIndex(
                name: "IX_StoreMenuItemAvailability_BrandMenuItemId",
                table: "StoreMenuItemAvailability");

            migrationBuilder.DropIndex(
                name: "IX_StoreMenuAssignments_BrandMenuId",
                table: "StoreMenuAssignments");

            migrationBuilder.CreateTable(
                name: "Collections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BrandId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActiveByBrand = table.Column<bool>(type: "bit", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CollectionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CollectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: true),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: true),
                    ProductVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionItems_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionItems_CollectionId",
                table: "CollectionItems",
                column: "CollectionId");
        }
    }
}
