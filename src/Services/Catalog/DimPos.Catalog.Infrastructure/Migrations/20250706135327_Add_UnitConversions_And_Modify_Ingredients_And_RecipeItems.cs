using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_UnitConversions_And_Modify_Ingredients_And_RecipeItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeItems_Recipes_RecipeId",
                table: "RecipeItems");

            migrationBuilder.DropTable(
                name: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_RecipeItems_RecipeId",
                table: "RecipeItems");

            migrationBuilder.DropColumn(
                name: "CostPerUnit",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Ingredients");

            migrationBuilder.RenameColumn(
                name: "RecipeId",
                table: "RecipeItems",
                newName: "ProductVariantId");

            migrationBuilder.RenameColumn(
                name: "MeasureUnit",
                table: "RecipeItems",
                newName: "UnitOfMeasureSnapshot");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "RecipeItems",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByAccountId",
                table: "RecipeItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "RecipeItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Ingredients",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MeasureUnit",
                table: "Ingredients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "BrandId",
                table: "Ingredients",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Ingredients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "Ingredients",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UnitConversions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ToUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Factor = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitConversions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitConversions_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeItems_ProductVariantId_IngredientId",
                table: "RecipeItems",
                columns: new[] { "ProductVariantId", "IngredientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_BrandId_Sku",
                table: "Ingredients",
                columns: new[] { "BrandId", "Sku" },
                unique: true,
                filter: "[Sku] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_Code",
                table: "Ingredients",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UnitConversions_IngredientId",
                table: "UnitConversions",
                column: "IngredientId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeItems_ProductVariants_ProductVariantId",
                table: "RecipeItems",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeItems_ProductVariants_ProductVariantId",
                table: "RecipeItems");

            migrationBuilder.DropTable(
                name: "UnitConversions");

            migrationBuilder.DropIndex(
                name: "IX_RecipeItems_ProductVariantId_IngredientId",
                table: "RecipeItems");

            migrationBuilder.DropIndex(
                name: "IX_Ingredients_BrandId_Sku",
                table: "Ingredients");

            migrationBuilder.DropIndex(
                name: "IX_Ingredients_Code",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "CreatedByAccountId",
                table: "RecipeItems");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "RecipeItems");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "Ingredients");

            migrationBuilder.RenameColumn(
                name: "UnitOfMeasureSnapshot",
                table: "RecipeItems",
                newName: "MeasureUnit");

            migrationBuilder.RenameColumn(
                name: "ProductVariantId",
                table: "RecipeItems",
                newName: "RecipeId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "RecipeItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Ingredients",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "MeasureUnit",
                table: "Ingredients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<Guid>(
                name: "BrandId",
                table: "Ingredients",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<decimal>(
                name: "CostPerUnit",
                table: "Ingredients",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Ingredients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Ingredients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recipes_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeItems_RecipeId",
                table: "RecipeItems",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_ProductVariantId",
                table: "Recipes",
                column: "ProductVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeItems_Recipes_RecipeId",
                table: "RecipeItems",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
