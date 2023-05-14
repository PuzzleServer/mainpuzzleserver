using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class Unlockstate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SinglePlayerPuzzleUnlockStates");
        }
    }
}
