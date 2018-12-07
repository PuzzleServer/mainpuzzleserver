using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class Hints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HintCoinCount",
                table: "Teams",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HintCoinsForSolve",
                table: "Puzzles",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Hints",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PuzzleID = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Content = table.Column<string>(nullable: false),
                    Cost = table.Column<int>(nullable: false),
                    DisplayOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hints_Puzzles_PuzzleID",
                        column: x => x.PuzzleID,
                        principalTable: "Puzzles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HintStatePerTeam",
                columns: table => new
                {
                    TeamID = table.Column<int>(nullable: false),
                    HintID = table.Column<int>(nullable: false),
                    UnlockTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HintStatePerTeam", x => new { x.TeamID, x.HintID });
                    table.ForeignKey(
                        name: "FK_HintStatePerTeam_Hints_HintID",
                        column: x => x.HintID,
                        principalTable: "Hints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HintStatePerTeam_Teams_TeamID",
                        column: x => x.TeamID,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hints_PuzzleID",
                table: "Hints",
                column: "PuzzleID");

            migrationBuilder.CreateIndex(
                name: "IX_HintStatePerTeam_HintID",
                table: "HintStatePerTeam",
                column: "HintID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HintStatePerTeam");

            migrationBuilder.DropTable(
                name: "Hints");

            migrationBuilder.DropColumn(
                name: "HintCoinCount",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "HintCoinsForSolve",
                table: "Puzzles");
        }
    }
}
