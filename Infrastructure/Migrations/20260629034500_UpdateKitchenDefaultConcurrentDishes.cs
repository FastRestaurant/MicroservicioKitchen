using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260629034500_UpdateKitchenDefaultConcurrentDishes")]
    public partial class UpdateKitchenDefaultConcurrentDishes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE KitchenConfigurations SET MaxConcurrentDishes = 20 WHERE MaxConcurrentDishes = 10");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE KitchenConfigurations SET MaxConcurrentDishes = 10 WHERE MaxConcurrentDishes = 20");
        }
    }
}
