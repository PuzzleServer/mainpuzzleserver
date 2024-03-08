using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMarkerOfRead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MarkReadUserID",
                table: "GeneralMessages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GeneralMessages_MarkReadUserID",
                table: "GeneralMessages",
                column: "MarkReadUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_GeneralMessages_PuzzleUsers_MarkReadUserID",
                table: "GeneralMessages",
                column: "MarkReadUserID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeneralMessages_PuzzleUsers_MarkReadUserID",
                table: "GeneralMessages");

            migrationBuilder.DropIndex(
                name: "IX_GeneralMessages_MarkReadUserID",
                table: "GeneralMessages");

            migrationBuilder.DropColumn(
                name: "MarkReadUserID",
                table: "GeneralMessages");
        }
    }
}
