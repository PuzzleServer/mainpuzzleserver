using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class Singleplayerpuzzle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsForSinglePlayer",
                table: "Puzzles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SinglePlayerPuzzleStatePerPlayer",
                columns: table => new
                {
                    PuzzleID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    UnlockedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SolvedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Printed = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LockoutExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsEmailOnlyMode = table.Column<bool>(type: "bit", nullable: false),
                    WrongSubmissionCountBuffer = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SinglePlayerPuzzleStatePerPlayer", x => new { x.PuzzleID, x.UserID });
                    table.ForeignKey(
                        name: "FK_SinglePlayerPuzzleStatePerPlayer_PuzzleUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "PuzzleUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SinglePlayerPuzzleStatePerPlayer_Puzzles_PuzzleID",
                        column: x => x.PuzzleID,
                        principalTable: "Puzzles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SinglePlayerPuzzleSubmissions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PuzzleID = table.Column<int>(type: "int", nullable: false),
                    SubmitterID = table.Column<int>(type: "int", nullable: false),
                    TimeSubmitted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmissionText = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ResponseID = table.Column<int>(type: "int", nullable: true),
                    FreeformResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FreeformAccepted = table.Column<bool>(type: "bit", nullable: true),
                    FreeformJudgeID = table.Column<int>(type: "int", nullable: true),
                    AllowFreeformSharing = table.Column<bool>(type: "bit", nullable: false),
                    FreeformFavorited = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SinglePlayerPuzzleSubmissions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SinglePlayerPuzzleSubmissions_PuzzleUsers_FreeformJudgeID",
                        column: x => x.FreeformJudgeID,
                        principalTable: "PuzzleUsers",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_SinglePlayerPuzzleSubmissions_PuzzleUsers_SubmitterID",
                        column: x => x.SubmitterID,
                        principalTable: "PuzzleUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SinglePlayerPuzzleSubmissions_Puzzles_PuzzleID",
                        column: x => x.PuzzleID,
                        principalTable: "Puzzles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SinglePlayerPuzzleSubmissions_Responses_ResponseID",
                        column: x => x.ResponseID,
                        principalTable: "Responses",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SinglePlayerPuzzleStatePerPlayer_UserID",
                table: "SinglePlayerPuzzleStatePerPlayer",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_SinglePlayerPuzzleStatePerPlayer_UserID_SolvedTime",
                table: "SinglePlayerPuzzleStatePerPlayer",
                columns: new[] { "UserID", "SolvedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SinglePlayerPuzzleSubmissions_FreeformJudgeID",
                table: "SinglePlayerPuzzleSubmissions",
                column: "FreeformJudgeID");

            migrationBuilder.CreateIndex(
                name: "IX_SinglePlayerPuzzleSubmissions_PuzzleID",
                table: "SinglePlayerPuzzleSubmissions",
                column: "PuzzleID");

            migrationBuilder.CreateIndex(
                name: "IX_SinglePlayerPuzzleSubmissions_ResponseID",
                table: "SinglePlayerPuzzleSubmissions",
                column: "ResponseID");

            migrationBuilder.CreateIndex(
                name: "IX_SinglePlayerPuzzleSubmissions_SubmitterID_PuzzleID_SubmissionText",
                table: "SinglePlayerPuzzleSubmissions",
                columns: new[] { "SubmitterID", "PuzzleID", "SubmissionText" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SinglePlayerPuzzleStatePerPlayer");

            migrationBuilder.DropTable(
                name: "SinglePlayerPuzzleSubmissions");

            migrationBuilder.DropColumn(
                name: "IsForSinglePlayer",
                table: "Puzzles");
        }
    }
}
