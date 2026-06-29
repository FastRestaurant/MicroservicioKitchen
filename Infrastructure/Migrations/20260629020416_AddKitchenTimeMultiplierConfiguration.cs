using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKitchenTimeMultiplierConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MaxQuantityTimeMultiplier",
                table: "KitchenOrderItems",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 3m);

            migrationBuilder.AddColumn<decimal>(
                name: "FactorMultiplierTime",
                table: "KitchenConfigurations",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0.5m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxQuantityTimeMultiplier",
                table: "KitchenConfigurations",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 3m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxQuantityTimeMultiplier",
                table: "KitchenOrderItems");

            migrationBuilder.DropColumn(
                name: "FactorMultiplierTime",
                table: "KitchenConfigurations");

            migrationBuilder.DropColumn(
                name: "MaxQuantityTimeMultiplier",
                table: "KitchenConfigurations");
        }
    }
}
