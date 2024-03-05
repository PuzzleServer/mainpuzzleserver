using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeOptionalParams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_PuzzleUsers_ClaimerID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Puzzles_PuzzleID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Teams_TeamID",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_EventID",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ThreadId",
                table: "Messages");

            migrationBuilder.AlterColumn<int>(
                name: "TeamID",
                table: "Messages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PuzzleID",
                table: "Messages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ClaimerID",
                table: "Messages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_EventID_ThreadId",
                table: "Messages",
                columns: new[] { "EventID", "ThreadId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_PuzzleUsers_ClaimerID",
                table: "Messages",
                column: "ClaimerID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_PuzzleUsers_ClaimerID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Puzzles_PuzzleID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Teams_TeamID",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_EventID_ThreadId",
                table: "Messages");

            migrationBuilder.AlterColumn<int>(
                name: "TeamID",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PuzzleID",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ClaimerID",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_EventID",
                table: "Messages",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ThreadId",
                table: "Messages",
                column: "ThreadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_PuzzleUsers_ClaimerID",
                table: "Messages",
                column: "ClaimerID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Puzzles_PuzzleID",
                table: "Messages",
                column: "PuzzleID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Teams_TeamID",
                table: "Messages",
                column: "TeamID",
                principalTable: "Teams",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
