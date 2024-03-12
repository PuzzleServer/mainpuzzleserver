using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class ProperCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Events_EventID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_PuzzleUsers_ClaimerID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_PuzzleUsers_SenderID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Puzzles_PuzzleID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Teams_TeamID",
                table: "Messages");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Events_EventID",
                table: "Messages",
                column: "EventID",
                principalTable: "Events",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_PuzzleUsers_ClaimerID",
                table: "Messages",
                column: "ClaimerID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_PuzzleUsers_SenderID",
                table: "Messages",
                column: "SenderID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Puzzles_PuzzleID",
                table: "Messages",
                column: "PuzzleID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Teams_TeamID",
                table: "Messages",
                column: "TeamID",
                principalTable: "Teams",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Events_EventID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_PuzzleUsers_ClaimerID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_PuzzleUsers_SenderID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Puzzles_PuzzleID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Teams_TeamID",
                table: "Messages");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Events_EventID",
                table: "Messages",
                column: "EventID",
                principalTable: "Events",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_PuzzleUsers_ClaimerID",
                table: "Messages",
                column: "ClaimerID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_PuzzleUsers_SenderID",
                table: "Messages",
                column: "SenderID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Puzzles_PuzzleID",
                table: "Messages",
                column: "PuzzleID",
                principalTable: "Puzzles",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Teams_TeamID",
                table: "Messages",
                column: "TeamID",
                principalTable: "Teams",
                principalColumn: "ID");
        }
    }
}
