using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class CustomURLs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomURL",
                table: "Puzzles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomURL",
                table: "Puzzles");
        }
    }
}
