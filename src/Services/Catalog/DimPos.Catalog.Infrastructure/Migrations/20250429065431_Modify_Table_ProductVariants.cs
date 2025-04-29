using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Modify_Table_ProductVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_ProductVariants_ProductVariantId",
                table: "Recipes");

            migrationBuilder.RenameColumn(
                name: "IsProductPriceBasedOnVariant",
                table: "ProductVariants",
                newName: "IsActive");

            migrationBuilder.AddColumn<string>(
                name: "AlternativeCode",
                table: "ProductVariants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ProductVariants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "ProductVariants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ProductVariants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceCOGS",
                table: "ProductVariants",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_Code",
                table: "ProductVariants",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_ProductVariants_ProductVariantId",
                table: "Recipes",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_ProductVariants_ProductVariantId",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_Code",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "AlternativeCode",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "PriceCOGS",
                table: "ProductVariants");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "ProductVariants",
                newName: "IsProductPriceBasedOnVariant");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_ProductVariants_ProductVariantId",
                table: "Recipes",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
