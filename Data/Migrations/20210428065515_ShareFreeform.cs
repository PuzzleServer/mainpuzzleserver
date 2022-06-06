using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class ShareFreeform : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowFreeformSharing",
                table: "Submissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FreeformFavorited",
                table: "Submissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "FreeformJudgeID",
                table: "Submissions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_FreeformJudgeID",
                table: "Submissions",
                column: "FreeformJudgeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_PuzzleUsers_FreeformJudgeID",
                table: "Submissions",
                column: "FreeformJudgeID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_PuzzleUsers_FreeformJudgeID",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_FreeformJudgeID",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "AllowFreeformSharing",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "FreeformFavorited",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "FreeformJudgeID",
                table: "Submissions");
        }
    }
}
