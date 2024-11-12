using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHelpThreadCoinState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHelpThreadUnlockedByCoins",
                table: "SinglePlayerPuzzleStatePerPlayer",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHelpThreadUnlockedByCoins",
                table: "PuzzleStatePerTeam",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHelpThreadUnlockedByCoins",
                table: "SinglePlayerPuzzleStatePerPlayer");

            migrationBuilder.DropColumn(
                name: "IsHelpThreadUnlockedByCoins",
                table: "PuzzleStatePerTeam");
        }
    }
}
