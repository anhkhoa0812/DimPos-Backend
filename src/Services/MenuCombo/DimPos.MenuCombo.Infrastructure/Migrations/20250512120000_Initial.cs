using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.MenuCombo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Collections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActiveByBrand = table.Column<bool>(type: "bit", nullable: true),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BrandId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Menu",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActiveByBrand = table.Column<bool>(type: "bit", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BrandId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoreMenuAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActiveAtStore = table.Column<bool>(type: "bit", nullable: true),
                    EffectiveAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BrandMenuId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreMenuAssignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CollectionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: true),
                    ProductVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CollectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: true),
                    ItemType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProductVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MenuId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_Menu_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StoreMenuItemAvailability",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActiveAtStore = table.Column<bool>(type: "bit", nullable: true),
                    EffectiveAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BrandMenuItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StoreMenuAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreMenuItemAvailability", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreMenuItemAvailability_StoreMenuAssignments_StoreMenuAssignmentId",
                        column: x => x.StoreMenuAssignmentId,
                        principalTable: "StoreMenuAssignments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionItems_CollectionId",
                table: "CollectionItems",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_MenuId",
                table: "MenuItems",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreMenuItemAvailability_StoreMenuAssignmentId",
                table: "StoreMenuItemAvailability",
                column: "StoreMenuAssignmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionItems");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "StoreMenuItemAvailability");

            migrationBuilder.DropTable(
                name: "Collections");

            migrationBuilder.DropTable(
                name: "Menu");

            migrationBuilder.DropTable(
                name: "StoreMenuAssignments");
        }
    }
}
