using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Order.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Table_OrderItemSelectedOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppliedOrderPromotions_OrderItems_OrderItemsId",
                table: "AppliedOrderPromotions");

            migrationBuilder.DropIndex(
                name: "IX_AppliedOrderPromotions_OrderItemsId",
                table: "AppliedOrderPromotions");

            migrationBuilder.DropColumn(
                name: "OrderItemsId",
                table: "AppliedOrderPromotions");

            migrationBuilder.CreateTable(
                name: "OrderItemSelectedOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifierGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifierOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifierGroupSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ModifierOptionSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PriceDeltaOptionSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemSelectedOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItemSelectedOptions_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemSelectedOptions_OrderItemId",
                table: "OrderItemSelectedOptions",
                column: "OrderItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItemSelectedOptions");

            migrationBuilder.AddColumn<Guid>(
                name: "OrderItemsId",
                table: "AppliedOrderPromotions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppliedOrderPromotions_OrderItemsId",
                table: "AppliedOrderPromotions",
                column: "OrderItemsId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedOrderPromotions_OrderItems_OrderItemsId",
                table: "AppliedOrderPromotions",
                column: "OrderItemsId",
                principalTable: "OrderItems",
                principalColumn: "Id");
        }
    }
}
