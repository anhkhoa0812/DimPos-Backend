using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Promotion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Channel_For_CampaignRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MetaData",
                table: "Campaigns");

            migrationBuilder.AddColumn<string>(
                name: "Channel",
                table: "Campaigns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Channel",
                table: "Campaigns");

            migrationBuilder.AddColumn<string>(
                name: "MetaData",
                table: "Campaigns",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
