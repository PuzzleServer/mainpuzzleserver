using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class PsptIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PuzzleStatePerTeam_TeamID_SolvedTime",
                table: "PuzzleStatePerTeam",
                columns: new[] { "TeamID", "SolvedTime" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PuzzleStatePerTeam_TeamID_SolvedTime",
                table: "PuzzleStatePerTeam");
        }
    }
}
