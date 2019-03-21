using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class RenameErrata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Erata",
                table: "Puzzles",
                newName: "Errata");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Errata",
                table: "Puzzles",
                newName: "Erata");
        }
    }
}
