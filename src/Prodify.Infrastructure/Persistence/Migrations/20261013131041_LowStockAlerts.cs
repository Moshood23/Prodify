using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prodify.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Low stock warnings. Existing variants warn at 5 units or fewer, like new ones.
    /// </summary>
    public partial class LowStockAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LowStockEmailSentAt",
                table: "Sellers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LowStockAlertSent",
                table: "ProductVariants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LowStockThreshold",
                table: "ProductVariants",
                type: "int",
                nullable: false,
                defaultValue: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LowStockEmailSentAt",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "LowStockAlertSent",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "LowStockThreshold",
                table: "ProductVariants");
        }
    }
}
