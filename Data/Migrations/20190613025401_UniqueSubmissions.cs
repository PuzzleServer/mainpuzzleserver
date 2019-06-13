using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class UniqueSubmissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Submissions_TeamID",
                table: "Submissions");

            migrationBuilder.AlterColumn<string>(
                name: "SubmissionText",
                table: "Submissions",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_TeamID_PuzzleID_SubmissionText",
                table: "Submissions",
                columns: new[] { "TeamID", "PuzzleID", "SubmissionText" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Submissions_TeamID_PuzzleID_SubmissionText",
                table: "Submissions");

            migrationBuilder.AlterColumn<string>(
                name: "SubmissionText",
                table: "Submissions",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_TeamID",
                table: "Submissions",
                column: "TeamID");
        }
    }
}
