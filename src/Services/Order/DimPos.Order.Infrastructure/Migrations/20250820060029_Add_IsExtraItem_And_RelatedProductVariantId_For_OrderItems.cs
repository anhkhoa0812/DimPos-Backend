using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Order.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IsExtraItem_And_RelatedProductVariantId_For_OrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceDeltaOptionSnapshot",
                table: "OrderItemSelectedOptions");

            migrationBuilder.AddColumn<bool>(
                name: "IsExtraItem",
                table: "OrderItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedProductVariantId",
                table: "OrderItems",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsExtraItem",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "RelatedProductVariantId",
                table: "OrderItems");

            migrationBuilder.AddColumn<decimal>(
                name: "PriceDeltaOptionSnapshot",
                table: "OrderItemSelectedOptions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
