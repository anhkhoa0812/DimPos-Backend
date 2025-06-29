using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Store.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_FinancialShiftConfigs_And_Modify_FinancialShifts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialShifts_Store_StoreId",
                table: "FinancialShifts");

            migrationBuilder.DropColumn(
                name: "ClosingCashActualCounted",
                table: "FinancialShifts");

            migrationBuilder.DropColumn(
                name: "ClosingCashSystemCalculated",
                table: "FinancialShifts");

            migrationBuilder.DropColumn(
                name: "ClosingDifference",
                table: "FinancialShifts");

            migrationBuilder.DropColumn(
                name: "ClosingDifferenceReason",
                table: "FinancialShifts");

            migrationBuilder.AlterColumn<Guid>(
                name: "StoreId",
                table: "FinancialShifts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "FinancialShiftConfigId",
                table: "FinancialShifts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "FinancialShiftConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpeningTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    ClosingTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    CreatedByAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialShiftConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialShiftConfigs_Store_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialShifts_FinancialShiftConfigId",
                table: "FinancialShifts",
                column: "FinancialShiftConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialShiftConfigs_StoreId",
                table: "FinancialShiftConfigs",
                column: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialShifts_FinancialShiftConfigs_FinancialShiftConfigId",
                table: "FinancialShifts",
                column: "FinancialShiftConfigId",
                principalTable: "FinancialShiftConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialShifts_Store_StoreId",
                table: "FinancialShifts",
                column: "StoreId",
                principalTable: "Store",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialShifts_FinancialShiftConfigs_FinancialShiftConfigId",
                table: "FinancialShifts");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialShifts_Store_StoreId",
                table: "FinancialShifts");

            migrationBuilder.DropTable(
                name: "FinancialShiftConfigs");

            migrationBuilder.DropIndex(
                name: "IX_FinancialShifts_FinancialShiftConfigId",
                table: "FinancialShifts");

            migrationBuilder.DropColumn(
                name: "FinancialShiftConfigId",
                table: "FinancialShifts");

            migrationBuilder.AlterColumn<Guid>(
                name: "StoreId",
                table: "FinancialShifts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosingCashActualCounted",
                table: "FinancialShifts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ClosingCashSystemCalculated",
                table: "FinancialShifts",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ClosingDifference",
                table: "FinancialShifts",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClosingDifferenceReason",
                table: "FinancialShifts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialShifts_Store_StoreId",
                table: "FinancialShifts",
                column: "StoreId",
                principalTable: "Store",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
