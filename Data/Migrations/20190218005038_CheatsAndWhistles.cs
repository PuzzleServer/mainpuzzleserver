using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class CheatsAndWhistles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCheatCode",
                table: "Puzzles",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MinutesOfEventLockout",
                table: "Puzzles",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCheatCode",
                table: "Puzzles");

            migrationBuilder.DropColumn(
                name: "MinutesOfEventLockout",
                table: "Puzzles");
        }
    }
}
