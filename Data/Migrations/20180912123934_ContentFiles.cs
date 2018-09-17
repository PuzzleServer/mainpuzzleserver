using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class ContentFiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswerUrlString",
                table: "Puzzles");

            migrationBuilder.DropColumn(
                name: "MaterialsUrlString",
                table: "Puzzles");

            migrationBuilder.DropColumn(
                name: "PuzzleUrlString",
                table: "Puzzles");

            migrationBuilder.CreateTable(
                name: "ContentFiles",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FileType = table.Column<int>(nullable: false),
                    EventID = table.Column<int>(nullable: false),
                    ShortName = table.Column<string>(nullable: false),
                    UrlString = table.Column<string>(nullable: false),
                    PuzzleID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentFiles", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ContentFiles_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentFiles_Puzzles_PuzzleID",
                        column: x => x.PuzzleID,
                        principalTable: "Puzzles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentFiles_PuzzleID",
                table: "ContentFiles",
                column: "PuzzleID");

            migrationBuilder.CreateIndex(
                name: "IX_ContentFiles_EventID_ShortName",
                table: "ContentFiles",
                columns: new[] { "EventID", "ShortName" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentFiles");

            migrationBuilder.AddColumn<string>(
                name: "AnswerUrlString",
                table: "Puzzles",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialsUrlString",
                table: "Puzzles",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PuzzleUrlString",
                table: "Puzzles",
                nullable: true);
        }
    }
}
