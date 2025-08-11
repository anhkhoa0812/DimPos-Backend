using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Order.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_RelatedComboProductVariant_For_OrderItemSelectedOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RelatedComboProductVariantItemId",
                table: "OrderItemSelectedOptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedComboProductVariantItemName",
                table: "OrderItemSelectedOptions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelatedComboProductVariantItemId",
                table: "OrderItemSelectedOptions");

            migrationBuilder.DropColumn(
                name: "RelatedComboProductVariantItemName",
                table: "OrderItemSelectedOptions");
        }
    }
}
