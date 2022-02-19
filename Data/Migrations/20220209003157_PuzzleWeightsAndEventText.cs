using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class PuzzleWeightsAndEventText : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PieceMetaTagFilter",
                table: "Puzzles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PieceTag",
                table: "Puzzles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PieceWeight",
                table: "Puzzles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrerequisiteWeight",
                table: "Puzzles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Announcement",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FAQContent",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeContent",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RulesContent",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PieceMetaTagFilter",
                table: "Puzzles");

            migrationBuilder.DropColumn(
                name: "PieceTag",
                table: "Puzzles");

            migrationBuilder.DropColumn(
                name: "PieceWeight",
                table: "Puzzles");

            migrationBuilder.DropColumn(
                name: "PrerequisiteWeight",
                table: "Puzzles");

            migrationBuilder.DropColumn(
                name: "Announcement",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "FAQContent",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "HomeContent",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RulesContent",
                table: "Events");
        }
    }
}
