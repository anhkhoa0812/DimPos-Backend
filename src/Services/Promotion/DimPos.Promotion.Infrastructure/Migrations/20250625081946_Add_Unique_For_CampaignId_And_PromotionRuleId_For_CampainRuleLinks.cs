using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Promotion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Unique_For_CampaignId_And_PromotionRuleId_For_CampainRuleLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CampaignRuleLinks_CampaignId",
                table: "CampaignRuleLinks");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRuleLinks_CampaignId_PromotionRuleId",
                table: "CampaignRuleLinks",
                columns: new[] { "CampaignId", "PromotionRuleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CampaignRuleLinks_CampaignId_PromotionRuleId",
                table: "CampaignRuleLinks");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRuleLinks_CampaignId",
                table: "CampaignRuleLinks",
                column: "CampaignId");
        }
    }
}
