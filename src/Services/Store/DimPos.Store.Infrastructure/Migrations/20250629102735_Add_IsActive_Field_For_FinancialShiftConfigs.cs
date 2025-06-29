using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Store.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IsActive_Field_For_FinancialShiftConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "FinancialShiftConfigs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "FinancialShiftConfigs");
        }
    }
}
