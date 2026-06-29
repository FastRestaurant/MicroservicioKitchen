using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260629033000_UpdateKitchenDefaultTimeMultipliers")]
    public partial class UpdateKitchenDefaultTimeMultipliers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE KitchenConfigurations SET FactorMultiplierTime = 0.10, MaxQuantityTimeMultiplier = 2.00 WHERE (FactorMultiplierTime = 0.50 AND MaxQuantityTimeMultiplier = 3.00) OR (FactorMultiplierTime = 0.10 AND MaxQuantityTimeMultiplier = 3.00)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE KitchenConfigurations SET FactorMultiplierTime = 0.50, MaxQuantityTimeMultiplier = 3.00 WHERE FactorMultiplierTime = 0.10 AND MaxQuantityTimeMultiplier = 2.00");
        }
    }
}
