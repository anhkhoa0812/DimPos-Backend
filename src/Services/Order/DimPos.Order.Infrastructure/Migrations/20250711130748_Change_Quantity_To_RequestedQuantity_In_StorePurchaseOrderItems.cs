using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Order.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Change_Quantity_To_RequestedQuantity_In_StorePurchaseOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceivedQuantityByStore",
                table: "StorePurchaseOrderItems");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "StorePurchaseOrderItems",
                newName: "RequestedQuantity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestedQuantity",
                table: "StorePurchaseOrderItems",
                newName: "Quantity");

            migrationBuilder.AddColumn<decimal>(
                name: "ReceivedQuantityByStore",
                table: "StorePurchaseOrderItems",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
