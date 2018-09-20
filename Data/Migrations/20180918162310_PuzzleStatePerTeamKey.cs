using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class PuzzleStatePerTeamKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PuzzleStatePerTeam_Puzzles_PuzzleID",
                table: "PuzzleStatePerTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_PuzzleStatePerTeam_Teams_TeamID",
                table: "PuzzleStatePerTeam");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PuzzleStatePerTeam",
                table: "PuzzleStatePerTeam");

            migrationBuilder.DropIndex(
                name: "IX_PuzzleStatePerTeam_PuzzleID",
                table: "PuzzleStatePerTeam");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "PuzzleStatePerTeam");

            migrationBuilder.AlterColumn<int>(
                name: "TeamID",
                table: "PuzzleStatePerTeam",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PuzzleID",
                table: "PuzzleStatePerTeam",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PuzzleStatePerTeam",
                table: "PuzzleStatePerTeam",
                columns: new[] { "PuzzleID", "TeamID" });

            migrationBuilder.AddForeignKey(
                name: "FK_PuzzleStatePerTeam_Puzzles_PuzzleID",
                table: "PuzzleStatePerTeam",
                column: "PuzzleID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PuzzleStatePerTeam_Teams_TeamID",
                table: "PuzzleStatePerTeam",
                column: "TeamID",
                principalTable: "Teams",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PuzzleStatePerTeam_Puzzles_PuzzleID",
                table: "PuzzleStatePerTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_PuzzleStatePerTeam_Teams_TeamID",
                table: "PuzzleStatePerTeam");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PuzzleStatePerTeam",
                table: "PuzzleStatePerTeam");

            migrationBuilder.AlterColumn<int>(
                name: "TeamID",
                table: "PuzzleStatePerTeam",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "PuzzleID",
                table: "PuzzleStatePerTeam",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "ID",
                table: "PuzzleStatePerTeam",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PuzzleStatePerTeam",
                table: "PuzzleStatePerTeam",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_PuzzleStatePerTeam_PuzzleID",
                table: "PuzzleStatePerTeam",
                column: "PuzzleID");

            migrationBuilder.AddForeignKey(
                name: "FK_PuzzleStatePerTeam_Puzzles_PuzzleID",
                table: "PuzzleStatePerTeam",
                column: "PuzzleID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PuzzleStatePerTeam_Teams_TeamID",
                table: "PuzzleStatePerTeam",
                column: "TeamID",
                principalTable: "Teams",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
