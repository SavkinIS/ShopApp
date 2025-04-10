using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddBobbine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Yarns",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "Length",
                table: "Yarns",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "ToolsSize",
                table: "Yarns",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "WeightGramm",
                table: "Yarns",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.CreateTable(
                name: "YarnBobbins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YarnBobbins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YarnBobbins_Yarns_Id",
                        column: x => x.Id,
                        principalTable: "Yarns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YarnBobbins");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Yarns");

            migrationBuilder.DropColumn(
                name: "Length",
                table: "Yarns");

            migrationBuilder.DropColumn(
                name: "ToolsSize",
                table: "Yarns");

            migrationBuilder.DropColumn(
                name: "WeightGramm",
                table: "Yarns");
        }
    }
}
