using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Store.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_StorePaymentMethedConfig_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StorePaymentMethodConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SystemPaymentMethodTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CredentialsConfigAtStore = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActiveByStore = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorePaymentMethodConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorePaymentMethodConfigs_Store_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StorePaymentMethodConfigs_StoreId",
                table: "StorePaymentMethodConfigs",
                column: "StoreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StorePaymentMethodConfigs");
        }
    }
}
