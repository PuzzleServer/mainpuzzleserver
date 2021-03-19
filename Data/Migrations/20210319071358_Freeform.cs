using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class Freeform : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FreeformAccepted",
                table: "Submissions",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FreeformResponse",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFreeform",
                table: "Puzzles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FreeformAccepted",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "FreeformResponse",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "IsFreeform",
                table: "Puzzles");
        }
    }
}
