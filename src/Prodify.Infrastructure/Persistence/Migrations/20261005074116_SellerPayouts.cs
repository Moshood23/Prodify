using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prodify.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SellerPayouts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayoutAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SellerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayoutAccounts_Sellers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlatformSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommissionRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SellerEarnings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SellerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SellerOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sales = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommissionRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AvailableAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerEarnings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellerEarnings_SellerOrders_SellerOrderId",
                        column: x => x.SellerOrderId,
                        principalTable: "SellerOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SellerEarnings_Sellers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SellerPayouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SellerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RejectReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerPayouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellerPayouts_Sellers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayoutAccounts_SellerId",
                table: "PayoutAccounts",
                column: "SellerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SellerEarnings_SellerId_AvailableAt",
                table: "SellerEarnings",
                columns: new[] { "SellerId", "AvailableAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SellerEarnings_SellerOrderId",
                table: "SellerEarnings",
                column: "SellerOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SellerPayouts_SellerId_OpenRequest",
                table: "SellerPayouts",
                column: "SellerId",
                unique: true,
                filter: "[Status] = 'Requested'");

            migrationBuilder.CreateIndex(
                name: "IX_SellerPayouts_Status_CreatedAt",
                table: "SellerPayouts",
                columns: new[] { "Status", "CreatedAt" });

            // Parts delivered before payouts existed count as earnings at the default 10%.
            migrationBuilder.Sql(@"
INSERT INTO SellerEarnings (Id, SellerId, SellerOrderId, OrderId, Sales, CommissionRate, Commission, NetAmount, EarnedAt, AvailableAt)
SELECT NEWID(), so.SellerId, so.Id, so.OrderId, s.Sales, 10.00,
       ROUND(s.Sales * 0.10, 2), s.Sales - ROUND(s.Sales * 0.10, 2), d.EarnedAt, DATEADD(day, 7, d.EarnedAt)
FROM SellerOrders so
CROSS APPLY (SELECT COALESCE(SUM(i.UnitPriceAmount * i.Quantity), 0) AS Sales
             FROM SellerOrderItems i WHERE i.SellerOrderId = so.Id) s
CROSS APPLY (SELECT COALESCE(MAX(h.OccurredAt), so.CreatedAt) AS EarnedAt
             FROM OrderStatusHistories h WHERE h.SellerOrderId = so.Id AND h.Status = 'Delivered') d
WHERE so.Status = 'Delivered';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayoutAccounts");

            migrationBuilder.DropTable(
                name: "PlatformSettings");

            migrationBuilder.DropTable(
                name: "SellerEarnings");

            migrationBuilder.DropTable(
                name: "SellerPayouts");
        }
    }
}