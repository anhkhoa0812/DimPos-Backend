using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Promotion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Unique_For_CampaignId_And_StoreId_For_CampainStores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CampaignStores_CampaignId",
                table: "CampaignStores");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignStores_CampaignId_StoreId",
                table: "CampaignStores",
                columns: new[] { "CampaignId", "StoreId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CampaignStores_CampaignId_StoreId",
                table: "CampaignStores");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignStores_CampaignId",
                table: "CampaignStores",
                column: "CampaignId");
        }
    }
}
