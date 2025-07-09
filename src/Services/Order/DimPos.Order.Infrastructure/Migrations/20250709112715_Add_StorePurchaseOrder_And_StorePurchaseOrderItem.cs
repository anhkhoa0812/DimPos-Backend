using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DimPos.Order.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_StorePurchaseOrder_And_StorePurchaseOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StorePurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BrandId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CancellationRequestReasonByStore = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CancellationReasonByBrand = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NoteFromStore = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NoteFromBrand = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EstimatedTotalValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConfirmedByBrandAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorePurchaseOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StorePurchaseOrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StorePurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductVariantIdSnapshot = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductVariantNameSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductVariantPriceSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPriceOfOrderItems = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitOfMeasure = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ApprovedQuantityByBrand = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReceivedQuantityByStore = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorePurchaseOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorePurchaseOrderItems_StorePurchaseOrders_StorePurchaseOrderId",
                        column: x => x.StorePurchaseOrderId,
                        principalTable: "StorePurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StorePurchaseOrderItems_StorePurchaseOrderId",
                table: "StorePurchaseOrderItems",
                column: "StorePurchaseOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StorePurchaseOrderItems");

            migrationBuilder.DropTable(
                name: "StorePurchaseOrders");
        }
    }
}
