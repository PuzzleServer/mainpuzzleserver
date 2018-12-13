using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class PuzzleAuthors_IDs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PuzzleAuthors_Puzzles_Puzzle.ID",
                table: "PuzzleAuthors");

            migrationBuilder.DropForeignKey(
                name: "FK_PuzzleAuthors_PuzzleUsers_User.ID",
                table: "PuzzleAuthors");

            migrationBuilder.DropIndex(
                name: "IX_PuzzleAuthors_Puzzle.ID",
                table: "PuzzleAuthors");

            migrationBuilder.DropIndex(
                name: "IX_PuzzleAuthors_User.ID",
                table: "PuzzleAuthors");

            migrationBuilder.DropColumn(
                name: "Puzzle.ID",
                table: "PuzzleAuthors");

            migrationBuilder.DropColumn(
                name: "User.ID",
                table: "PuzzleAuthors");

            migrationBuilder.AddColumn<int>(
                name: "AuthorID",
                table: "PuzzleAuthors",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PuzzleID",
                table: "PuzzleAuthors",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PuzzleAuthors_AuthorID",
                table: "PuzzleAuthors",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_PuzzleAuthors_PuzzleID",
                table: "PuzzleAuthors",
                column: "PuzzleID");

            migrationBuilder.AddForeignKey(
                name: "FK_PuzzleAuthors_PuzzleUsers_AuthorID",
                table: "PuzzleAuthors",
                column: "AuthorID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PuzzleAuthors_Puzzles_PuzzleID",
                table: "PuzzleAuthors",
                column: "PuzzleID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PuzzleAuthors_PuzzleUsers_AuthorID",
                table: "PuzzleAuthors");

            migrationBuilder.DropForeignKey(
                name: "FK_PuzzleAuthors_Puzzles_PuzzleID",
                table: "PuzzleAuthors");

            migrationBuilder.DropIndex(
                name: "IX_PuzzleAuthors_AuthorID",
                table: "PuzzleAuthors");

            migrationBuilder.DropIndex(
                name: "IX_PuzzleAuthors_PuzzleID",
                table: "PuzzleAuthors");

            migrationBuilder.DropColumn(
                name: "AuthorID",
                table: "PuzzleAuthors");

            migrationBuilder.DropColumn(
                name: "PuzzleID",
                table: "PuzzleAuthors");

            migrationBuilder.AddColumn<int>(
                name: "Puzzle.ID",
                table: "PuzzleAuthors",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "User.ID",
                table: "PuzzleAuthors",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PuzzleAuthors_Puzzle.ID",
                table: "PuzzleAuthors",
                column: "Puzzle.ID");

            migrationBuilder.CreateIndex(
                name: "IX_PuzzleAuthors_User.ID",
                table: "PuzzleAuthors",
                column: "User.ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PuzzleAuthors_Puzzles_Puzzle.ID",
                table: "PuzzleAuthors",
                column: "Puzzle.ID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PuzzleAuthors_PuzzleUsers_User.ID",
                table: "PuzzleAuthors",
                column: "User.ID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
