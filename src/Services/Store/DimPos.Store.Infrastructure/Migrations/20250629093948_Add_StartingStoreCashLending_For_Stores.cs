using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Store.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_StartingStoreCashLending_For_Stores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "StartingStoreCashLending",
                table: "Store",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartingStoreCashLending",
                table: "Store");
        }
    }
}
