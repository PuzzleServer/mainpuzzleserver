using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFromGameControl",
                table: "GeneralMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TeamID",
                table: "GeneralMessages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeamPuzzleMessage_TeamID",
                table: "GeneralMessages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeneralMessages_TeamID",
                table: "GeneralMessages",
                column: "TeamID");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralMessages_TeamPuzzleMessage_TeamID",
                table: "GeneralMessages",
                column: "TeamPuzzleMessage_TeamID");

            migrationBuilder.AddForeignKey(
                name: "FK_GeneralMessages_Teams_TeamID",
                table: "GeneralMessages",
                column: "TeamID",
                principalTable: "Teams",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GeneralMessages_Teams_TeamPuzzleMessage_TeamID",
                table: "GeneralMessages",
                column: "TeamPuzzleMessage_TeamID",
                principalTable: "Teams",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeneralMessages_Teams_TeamID",
                table: "GeneralMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_GeneralMessages_Teams_TeamPuzzleMessage_TeamID",
                table: "GeneralMessages");

            migrationBuilder.DropIndex(
                name: "IX_GeneralMessages_TeamID",
                table: "GeneralMessages");

            migrationBuilder.DropIndex(
                name: "IX_GeneralMessages_TeamPuzzleMessage_TeamID",
                table: "GeneralMessages");

            migrationBuilder.DropColumn(
                name: "IsFromGameControl",
                table: "GeneralMessages");

            migrationBuilder.DropColumn(
                name: "TeamID",
                table: "GeneralMessages");

            migrationBuilder.DropColumn(
                name: "TeamPuzzleMessage_TeamID",
                table: "GeneralMessages");
        }
    }
}
