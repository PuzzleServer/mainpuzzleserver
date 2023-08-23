using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSinglePlayerPuzzleHintCounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SinglePlayerPuzzleHintCoinCountPerPlayer",
                columns: table => new
                {
                    PuzzleUserID = table.Column<int>(type: "int", nullable: false),
                    EventID = table.Column<int>(type: "int", nullable: false),
                    HintCoinCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SinglePlayerPuzzleHintCoinCountPerPlayer", x => new { x.PuzzleUserID, x.EventID });
                    table.ForeignKey(
                        name: "FK_SinglePlayerPuzzleHintCoinCountPerPlayer_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SinglePlayerPuzzleHintCoinCountPerPlayer_PuzzleUsers_PuzzleUserID",
                        column: x => x.PuzzleUserID,
                        principalTable: "PuzzleUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SinglePlayerPuzzleHintCoinCountPerPlayer_EventID",
                table: "SinglePlayerPuzzleHintCoinCountPerPlayer",
                column: "EventID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SinglePlayerPuzzleHintCoinCountPerPlayer");
        }
    }
}
