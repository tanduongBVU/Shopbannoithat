using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopbannoithat.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColorId",
                table: "Carts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaterialId",
                table: "Carts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SizeId",
                table: "Carts",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "MaterialId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "SizeId",
                table: "Carts");
        }
    }
}
