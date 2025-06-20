using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Sku_For_Product_Variants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "ProductVariants",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sku",
                table: "ProductVariants");
        }
    }
}
