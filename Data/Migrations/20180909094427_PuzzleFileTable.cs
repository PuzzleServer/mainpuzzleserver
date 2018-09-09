using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class PuzzleFileTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PuzzleFiles",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UrlString = table.Column<string>(nullable: false),
                    IsAnswer = table.Column<bool>(nullable: false),
                    UnlocksWithPuzzleID = table.Column<int>(nullable: true),
                    UnlocksWithSolveID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuzzleFiles", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PuzzleFiles_Puzzles_UnlocksWithPuzzleID",
                        column: x => x.UnlocksWithPuzzleID,
                        principalTable: "Puzzles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PuzzleFiles_Puzzles_UnlocksWithSolveID",
                        column: x => x.UnlocksWithSolveID,
                        principalTable: "Puzzles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PuzzleFiles_UnlocksWithPuzzleID",
                table: "PuzzleFiles",
                column: "UnlocksWithPuzzleID");

            migrationBuilder.CreateIndex(
                name: "IX_PuzzleFiles_UnlocksWithSolveID",
                table: "PuzzleFiles",
                column: "UnlocksWithSolveID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PuzzleFiles");
        }
    }
}
