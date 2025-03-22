using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Weight",
                table: "Products",
                newName: "Color");

            migrationBuilder.AddColumn<float>(
                name: "WeightGramm",
                table: "Products",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeightGramm",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Color",
                table: "Products",
                newName: "Weight");
        }
    }
}
