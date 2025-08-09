using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Promotion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Remove_IsActiveAtStore_Of_CampaignStores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActiveAtStore",
                table: "CampaignStores");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActiveAtStore",
                table: "CampaignStores",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
