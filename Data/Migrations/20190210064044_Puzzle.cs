using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class Puzzle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxAnnotationKey",
                table: "Puzzles",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxAnnotationKey",
                table: "Puzzles");
        }
    }
}
