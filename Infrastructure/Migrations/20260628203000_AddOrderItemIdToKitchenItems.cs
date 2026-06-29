using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260628203000_AddOrderItemIdToKitchenItems")]
    public partial class AddOrderItemIdToKitchenItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrderItemId",
                table: "KitchenOrderItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.CreateIndex(
                name: "IX_KitchenOrderItems_OrderItemId",
                table: "KitchenOrderItems",
                column: "OrderItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KitchenOrderItems_OrderItemId",
                table: "KitchenOrderItems");

            migrationBuilder.DropColumn(
                name: "OrderItemId",
                table: "KitchenOrderItems");
        }
    }
}
