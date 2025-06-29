using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Store.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_TaxRates_And_FinancialShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialShifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpeningTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OpenedByAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpeningCashExpected = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OpeningCashActual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OpeningDifferenceReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ClosingTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedByAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClosingCashActualCounted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosingCashSystemCalculated = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ClosingDifference = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ClosingDifferenceReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalGrossSalesInShift = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalNetSalesInShift = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalTaxInShift = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalDiscountInShift = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalCashRoundingInShift = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialShifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialShifts_Store_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaxRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BrandId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxRates_Store_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialShifts_StoreId",
                table: "FinancialShifts",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_StoreId",
                table: "TaxRates",
                column: "StoreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialShifts");

            migrationBuilder.DropTable(
                name: "TaxRates");
        }
    }
}
