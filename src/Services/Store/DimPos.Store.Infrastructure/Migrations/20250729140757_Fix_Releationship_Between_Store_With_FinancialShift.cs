using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Store.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Releationship_Between_Store_With_FinancialShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialShifts_Store_StoreId",
                table: "FinancialShifts");

            migrationBuilder.DropIndex(
                name: "IX_FinancialShifts_StoreId",
                table: "FinancialShifts");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "FinancialShifts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "FinancialShifts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialShifts_StoreId",
                table: "FinancialShifts",
                column: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialShifts_Store_StoreId",
                table: "FinancialShifts",
                column: "StoreId",
                principalTable: "Store",
                principalColumn: "Id");
        }
    }
}
