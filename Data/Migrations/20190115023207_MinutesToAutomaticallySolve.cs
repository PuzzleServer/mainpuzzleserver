using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class MinutesToAutomaticallySolve : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinutesToAutomaticallySolve",
                table: "Puzzles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinutesToAutomaticallySolve",
                table: "Puzzles");
        }
    }
}
