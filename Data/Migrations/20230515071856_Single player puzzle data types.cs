using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class Singleplayerpuzzledatatypes : Migration
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
                name: "SinglePlayerPuzzleHintStatePerPlayer",
                columns: table => new
                {
                    PlayerID = table.Column<int>(type: "int", nullable: false),
                    HintID = table.Column<int>(type: "int", nullable: false),
                    UnlockTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SinglePlayerPuzzleHintStatePerPlayer", x => new { x.PlayerID, x.HintID });
                    table.ForeignKey(
                        name: "FK_SinglePlayerPuzzleHintStatePerPlayer_Hints_HintID",
                        column: x => x.HintID,
                        principalTable: "Hints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SinglePlayerPuzzleHintStatePerPlayer_PuzzleUsers_PlayerID",
                        column: x => x.PlayerID,
                        principalTable: "PuzzleUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SinglePlayerPuzzleStatePerPlayer",
                columns: table => new
                {
                    PuzzleID = table.Column<int>(type: "int", nullable: false),
                    PlayerID = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_SinglePlayerPuzzleStatePerPlayer", x => new { x.PuzzleID, x.PlayerID });
                    table.ForeignKey(
                        name: "FK_SinglePlayerPuzzleStatePerPlayer_PuzzleUsers_PlayerID",
                        column: x => x.PlayerID,
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

            migrationBuilder.CreateTable(
                name: "SinglePlayerPuzzleUnlockStates",
                columns: table => new
                {
                    PuzzleID = table.Column<int>(type: "int", nullable: false),
                    UnlockedTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SinglePlayerPuzzleUnlockStates", x => x.PuzzleID);
                    table.ForeignKey(
                        name: "FK_SinglePlayerPuzzleUnlockStates_Puzzles_PuzzleID",
                        column: x => x.PuzzleID,
                        principalTable: "Puzzles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SinglePlayerPuzzleHintStatePerPlayer_HintID",
                table: "SinglePlayerPuzzleHintStatePerPlayer",
                column: "HintID");

            migrationBuilder.CreateIndex(
                name: "IX_SinglePlayerPuzzleStatePerPlayer_PlayerID",
                table: "SinglePlayerPuzzleStatePerPlayer",
                column: "PlayerID");

            migrationBuilder.CreateIndex(
                name: "IX_SinglePlayerPuzzleStatePerPlayer_PlayerID_SolvedTime",
                table: "SinglePlayerPuzzleStatePerPlayer",
                columns: new[] { "PlayerID", "SolvedTime" });

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
                name: "SinglePlayerPuzzleHintStatePerPlayer");

            migrationBuilder.DropTable(
                name: "SinglePlayerPuzzleStatePerPlayer");

            migrationBuilder.DropTable(
                name: "SinglePlayerPuzzleSubmissions");

            migrationBuilder.DropTable(
                name: "SinglePlayerPuzzleUnlockStates");

            migrationBuilder.DropColumn(
                name: "IsForSinglePlayer",
                table: "Puzzles");
        }
    }
}
