using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class AdminFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MayBeAdminOrAuthor",
                table: "PuzzleUsers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MayBeAdminOrAuthor",
                table: "PuzzleUsers");
        }
    }
}
