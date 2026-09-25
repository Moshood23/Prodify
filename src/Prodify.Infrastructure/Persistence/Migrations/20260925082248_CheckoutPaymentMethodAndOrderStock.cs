using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prodify.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CheckoutPaymentMethodAndOrderStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "StockReservations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Card");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_OrderId",
                table: "StockReservations",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockReservations_OrderId",
                table: "StockReservations");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "StockReservations");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Orders");
        }
    }
}
