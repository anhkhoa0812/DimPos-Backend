using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Order.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_AppliedTaxes_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalPrice",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ItemDiscountAmount",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "AppliedOrderPromotions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AppliedOrderPromotions");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "AppliedOrderPromotions");

            migrationBuilder.AddColumn<Guid>(
                name: "CancelledByAccountId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PromotionDescriptionSnapshot",
                table: "AppliedOrderPromotions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppliedTaxes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaxRateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaxNameSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TaxRateSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppliedTaxes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppliedTaxes_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppliedTaxes_OrderId",
                table: "AppliedTaxes",
                column: "OrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppliedTaxes");

            migrationBuilder.DropColumn(
                name: "CancelledByAccountId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PromotionDescriptionSnapshot",
                table: "AppliedOrderPromotions");

            migrationBuilder.AddColumn<decimal>(
                name: "FinalPrice",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ItemDiscountAmount",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "AppliedOrderPromotions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AppliedOrderPromotions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "AppliedOrderPromotions",
                type: "datetime2",
                nullable: true);
        }
    }
}
