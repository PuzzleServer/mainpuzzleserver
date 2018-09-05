using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class Plurals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Event_EventAdmins_AdminsID",
                table: "Event");

            migrationBuilder.DropForeignKey(
                name: "FK_EventAuthors_Event_Event.ID",
                table: "EventAuthors");

            migrationBuilder.DropForeignKey(
                name: "FK_EventOwners_Event_Event.ID",
                table: "EventOwners");

            migrationBuilder.DropForeignKey(
                name: "FK_EventTeams_Event_Event.ID",
                table: "EventTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_Puzzle_PuzzleID",
                table: "Feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_Puzzle_Event_EventID",
                table: "Puzzle");

            migrationBuilder.DropForeignKey(
                name: "FK_PuzzleAuthors_Puzzle_Puzzle.ID",
                table: "PuzzleAuthors");

            migrationBuilder.DropForeignKey(
                name: "FK_PuzzleStatePerTeam_Puzzle_PuzzleID",
                table: "PuzzleStatePerTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_Responses_Puzzle_PuzzleID",
                table: "Responses");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Puzzle_PuzzleID",
                table: "Submissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Puzzle",
                table: "Puzzle");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Event",
                table: "Event");

            migrationBuilder.RenameTable(
                name: "Puzzle",
                newName: "Puzzles");

            migrationBuilder.RenameTable(
                name: "Event",
                newName: "Events");

            migrationBuilder.RenameIndex(
                name: "IX_Puzzle_EventID",
                table: "Puzzles",
                newName: "IX_Puzzles_EventID");

            migrationBuilder.RenameIndex(
                name: "IX_Event_AdminsID",
                table: "Events",
                newName: "IX_Events_AdminsID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Puzzles",
                table: "Puzzles",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Events",
                table: "Events",
                column: "ID");

            migrationBuilder.CreateTable(
                name: "Prerequisites",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PuzzleID = table.Column<int>(nullable: true),
                    PrerequisiteID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prerequisites", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Prerequisites_Puzzles_PrerequisiteID",
                        column: x => x.PrerequisiteID,
                        principalTable: "Puzzles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prerequisites_Puzzles_PuzzleID",
                        column: x => x.PuzzleID,
                        principalTable: "Puzzles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prerequisites_PrerequisiteID",
                table: "Prerequisites",
                column: "PrerequisiteID");

            migrationBuilder.CreateIndex(
                name: "IX_Prerequisites_PuzzleID",
                table: "Prerequisites",
                column: "PuzzleID");

            migrationBuilder.AddForeignKey(
                name: "FK_EventAuthors_Events_Event.ID",
                table: "EventAuthors",
                column: "Event.ID",
                principalTable: "Events",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventOwners_Events_Event.ID",
                table: "EventOwners",
                column: "Event.ID",
                principalTable: "Events",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_EventAdmins_AdminsID",
                table: "Events",
                column: "AdminsID",
                principalTable: "EventAdmins",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTeams_Events_Event.ID",
                table: "EventTeams",
                column: "Event.ID",
                principalTable: "Events",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_Puzzles_PuzzleID",
                table: "Feedback",
                column: "PuzzleID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PuzzleAuthors_Puzzles_Puzzle.ID",
                table: "PuzzleAuthors",
                column: "Puzzle.ID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Puzzles_Events_EventID",
                table: "Puzzles",
                column: "EventID",
                principalTable: "Events",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PuzzleStatePerTeam_Puzzles_PuzzleID",
                table: "PuzzleStatePerTeam",
                column: "PuzzleID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Responses_Puzzles_PuzzleID",
                table: "Responses",
                column: "PuzzleID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Puzzles_PuzzleID",
                table: "Submissions",
                column: "PuzzleID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventAuthors_Events_Event.ID",
                table: "EventAuthors");

            migrationBuilder.DropForeignKey(
                name: "FK_EventOwners_Events_Event.ID",
                table: "EventOwners");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_EventAdmins_AdminsID",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_EventTeams_Events_Event.ID",
                table: "EventTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_Puzzles_PuzzleID",
                table: "Feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_PuzzleAuthors_Puzzles_Puzzle.ID",
                table: "PuzzleAuthors");

            migrationBuilder.DropForeignKey(
                name: "FK_Puzzles_Events_EventID",
                table: "Puzzles");

            migrationBuilder.DropForeignKey(
                name: "FK_PuzzleStatePerTeam_Puzzles_PuzzleID",
                table: "PuzzleStatePerTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_Responses_Puzzles_PuzzleID",
                table: "Responses");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Puzzles_PuzzleID",
                table: "Submissions");

            migrationBuilder.DropTable(
                name: "Prerequisites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Puzzles",
                table: "Puzzles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Events",
                table: "Events");

            migrationBuilder.RenameTable(
                name: "Puzzles",
                newName: "Puzzle");

            migrationBuilder.RenameTable(
                name: "Events",
                newName: "Event");

            migrationBuilder.RenameIndex(
                name: "IX_Puzzles_EventID",
                table: "Puzzle",
                newName: "IX_Puzzle_EventID");

            migrationBuilder.RenameIndex(
                name: "IX_Events_AdminsID",
                table: "Event",
                newName: "IX_Event_AdminsID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Puzzle",
                table: "Puzzle",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Event",
                table: "Event",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Event_EventAdmins_AdminsID",
                table: "Event",
                column: "AdminsID",
                principalTable: "EventAdmins",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventAuthors_Event_Event.ID",
                table: "EventAuthors",
                column: "Event.ID",
                principalTable: "Event",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventOwners_Event_Event.ID",
                table: "EventOwners",
                column: "Event.ID",
                principalTable: "Event",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTeams_Event_Event.ID",
                table: "EventTeams",
                column: "Event.ID",
                principalTable: "Event",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_Puzzle_PuzzleID",
                table: "Feedback",
                column: "PuzzleID",
                principalTable: "Puzzle",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Puzzle_Event_EventID",
                table: "Puzzle",
                column: "EventID",
                principalTable: "Event",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PuzzleAuthors_Puzzle_Puzzle.ID",
                table: "PuzzleAuthors",
                column: "Puzzle.ID",
                principalTable: "Puzzle",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PuzzleStatePerTeam_Puzzle_PuzzleID",
                table: "PuzzleStatePerTeam",
                column: "PuzzleID",
                principalTable: "Puzzle",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Responses_Puzzle_PuzzleID",
                table: "Responses",
                column: "PuzzleID",
                principalTable: "Puzzle",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Puzzle_PuzzleID",
                table: "Submissions",
                column: "PuzzleID",
                principalTable: "Puzzle",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
