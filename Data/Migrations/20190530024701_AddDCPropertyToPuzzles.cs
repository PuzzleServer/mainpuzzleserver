using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class AddDCPropertyToPuzzles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasDataConfirmation",
                table: "Puzzles",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasDataConfirmation",
                table: "Puzzles");
        }
    }
}
