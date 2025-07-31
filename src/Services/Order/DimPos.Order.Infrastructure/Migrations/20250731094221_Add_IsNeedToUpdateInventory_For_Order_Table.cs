using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Order.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IsNeedToUpdateInventory_For_Order_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNeedToUpdateInventory",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNeedToUpdateInventory",
                table: "Orders");
        }
    }
}
