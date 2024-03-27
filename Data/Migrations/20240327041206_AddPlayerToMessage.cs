using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerToMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlayerID",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_PlayerID",
                table: "Messages",
                column: "PlayerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_PuzzleUsers_PlayerID",
                table: "Messages",
                column: "PlayerID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_PuzzleUsers_PlayerID",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_PlayerID",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "PlayerID",
                table: "Messages");
        }
    }
}
