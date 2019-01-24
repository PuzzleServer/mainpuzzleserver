using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class DBIntegrity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventAdmins_Events_Event.ID",
                table: "EventAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_EventAdmins_PuzzleUsers_User.ID",
                table: "EventAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_EventAuthors_Events_Event.ID",
                table: "EventAuthors");

            migrationBuilder.DropForeignKey(
                name: "FK_EventAuthors_PuzzleUsers_User.ID",
                table: "EventAuthors");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_Puzzles_PuzzleID",
                table: "Feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_PuzzleUsers_SubmitterID",
                table: "Feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_Puzzles_Events_EventID",
                table: "Puzzles");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Puzzles_PuzzleID",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_PuzzleUsers_SubmitterID",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Teams_TeamID",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_Teams_Team.ID",
                table: "TeamMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_PuzzleUsers_User.ID",
                table: "TeamMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Events_EventID",
                table: "Teams");

            migrationBuilder.DropTable(
                name: "EventTeams");

            migrationBuilder.DropIndex(
                name: "IX_EventAuthors_Event.ID",
                table: "EventAuthors");

            migrationBuilder.DropIndex(
                name: "IX_EventAuthors_User.ID",
                table: "EventAuthors");

            migrationBuilder.DropIndex(
                name: "IX_EventAdmins_Event.ID",
                table: "EventAdmins");

            migrationBuilder.DropIndex(
                name: "IX_EventAdmins_User.ID",
                table: "EventAdmins");

            migrationBuilder.DropColumn(
                name: "Event.ID",
                table: "EventAuthors");

            migrationBuilder.DropColumn(
                name: "User.ID",
                table: "EventAuthors");

            migrationBuilder.DropColumn(
                name: "Event.ID",
                table: "EventAdmins");

            migrationBuilder.DropColumn(
                name: "User.ID",
                table: "EventAdmins");

            migrationBuilder.AlterColumn<int>(
                name: "EventID",
                table: "Teams",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "User.ID",
                table: "TeamMembers",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Team.ID",
                table: "TeamMembers",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TeamID",
                table: "Submissions",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SubmitterID",
                table: "Submissions",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PuzzleID",
                table: "Submissions",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EventID",
                table: "Puzzles",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SubmitterID",
                table: "Feedback",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PuzzleID",
                table: "Feedback",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AuthorID",
                table: "EventAuthors",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EventID",
                table: "EventAuthors",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AdminID",
                table: "EventAdmins",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EventID",
                table: "EventAdmins",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EventAuthors_AuthorID",
                table: "EventAuthors",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_EventAuthors_EventID",
                table: "EventAuthors",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_EventAdmins_AdminID",
                table: "EventAdmins",
                column: "AdminID");

            migrationBuilder.CreateIndex(
                name: "IX_EventAdmins_EventID",
                table: "EventAdmins",
                column: "EventID");

            migrationBuilder.AddForeignKey(
                name: "FK_EventAdmins_PuzzleUsers_AdminID",
                table: "EventAdmins",
                column: "AdminID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventAdmins_Events_EventID",
                table: "EventAdmins",
                column: "EventID",
                principalTable: "Events",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventAuthors_PuzzleUsers_AuthorID",
                table: "EventAuthors",
                column: "AuthorID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventAuthors_Events_EventID",
                table: "EventAuthors",
                column: "EventID",
                principalTable: "Events",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_Puzzles_PuzzleID",
                table: "Feedback",
                column: "PuzzleID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_PuzzleUsers_SubmitterID",
                table: "Feedback",
                column: "SubmitterID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Puzzles_Events_EventID",
                table: "Puzzles",
                column: "EventID",
                principalTable: "Events",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Puzzles_PuzzleID",
                table: "Submissions",
                column: "PuzzleID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_PuzzleUsers_SubmitterID",
                table: "Submissions",
                column: "SubmitterID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Teams_TeamID",
                table: "Submissions",
                column: "TeamID",
                principalTable: "Teams",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_Teams_Team.ID",
                table: "TeamMembers",
                column: "Team.ID",
                principalTable: "Teams",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_PuzzleUsers_User.ID",
                table: "TeamMembers",
                column: "User.ID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Events_EventID",
                table: "Teams",
                column: "EventID",
                principalTable: "Events",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventAdmins_PuzzleUsers_AdminID",
                table: "EventAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_EventAdmins_Events_EventID",
                table: "EventAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_EventAuthors_PuzzleUsers_AuthorID",
                table: "EventAuthors");

            migrationBuilder.DropForeignKey(
                name: "FK_EventAuthors_Events_EventID",
                table: "EventAuthors");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_Puzzles_PuzzleID",
                table: "Feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_PuzzleUsers_SubmitterID",
                table: "Feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_Puzzles_Events_EventID",
                table: "Puzzles");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Puzzles_PuzzleID",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_PuzzleUsers_SubmitterID",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Teams_TeamID",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_Teams_Team.ID",
                table: "TeamMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_PuzzleUsers_User.ID",
                table: "TeamMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Events_EventID",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_EventAuthors_AuthorID",
                table: "EventAuthors");

            migrationBuilder.DropIndex(
                name: "IX_EventAuthors_EventID",
                table: "EventAuthors");

            migrationBuilder.DropIndex(
                name: "IX_EventAdmins_AdminID",
                table: "EventAdmins");

            migrationBuilder.DropIndex(
                name: "IX_EventAdmins_EventID",
                table: "EventAdmins");

            migrationBuilder.DropColumn(
                name: "AuthorID",
                table: "EventAuthors");

            migrationBuilder.DropColumn(
                name: "EventID",
                table: "EventAuthors");

            migrationBuilder.DropColumn(
                name: "AdminID",
                table: "EventAdmins");

            migrationBuilder.DropColumn(
                name: "EventID",
                table: "EventAdmins");

            migrationBuilder.AlterColumn<int>(
                name: "EventID",
                table: "Teams",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "User.ID",
                table: "TeamMembers",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "Team.ID",
                table: "TeamMembers",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "TeamID",
                table: "Submissions",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "SubmitterID",
                table: "Submissions",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "PuzzleID",
                table: "Submissions",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "EventID",
                table: "Puzzles",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "SubmitterID",
                table: "Feedback",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "PuzzleID",
                table: "Feedback",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "Event.ID",
                table: "EventAuthors",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "User.ID",
                table: "EventAuthors",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Event.ID",
                table: "EventAdmins",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "User.ID",
                table: "EventAdmins",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EventTeams",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EventID = table.Column<int>(name: "Event.ID", nullable: true),
                    TeamsID = table.Column<int>(name: "Teams.ID", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTeams", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EventTeams_Events_Event.ID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventTeams_Teams_Teams.ID",
                        column: x => x.TeamsID,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventAuthors_Event.ID",
                table: "EventAuthors",
                column: "Event.ID",
                unique: true,
                filter: "[Event.ID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EventAuthors_User.ID",
                table: "EventAuthors",
                column: "User.ID");

            migrationBuilder.CreateIndex(
                name: "IX_EventAdmins_Event.ID",
                table: "EventAdmins",
                column: "Event.ID",
                unique: true,
                filter: "[Event.ID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EventAdmins_User.ID",
                table: "EventAdmins",
                column: "User.ID");

            migrationBuilder.CreateIndex(
                name: "IX_EventTeams_Event.ID",
                table: "EventTeams",
                column: "Event.ID",
                unique: true,
                filter: "[Event.ID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EventTeams_Teams.ID",
                table: "EventTeams",
                column: "Teams.ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EventAdmins_Events_Event.ID",
                table: "EventAdmins",
                column: "Event.ID",
                principalTable: "Events",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventAdmins_PuzzleUsers_User.ID",
                table: "EventAdmins",
                column: "User.ID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventAuthors_Events_Event.ID",
                table: "EventAuthors",
                column: "Event.ID",
                principalTable: "Events",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventAuthors_PuzzleUsers_User.ID",
                table: "EventAuthors",
                column: "User.ID",
                principalTable: "PuzzleUsers",
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
                name: "FK_Feedback_PuzzleUsers_SubmitterID",
                table: "Feedback",
                column: "SubmitterID",
                principalTable: "PuzzleUsers",
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
                name: "FK_Submissions_Puzzles_PuzzleID",
                table: "Submissions",
                column: "PuzzleID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_PuzzleUsers_SubmitterID",
                table: "Submissions",
                column: "SubmitterID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Teams_TeamID",
                table: "Submissions",
                column: "TeamID",
                principalTable: "Teams",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_Teams_Team.ID",
                table: "TeamMembers",
                column: "Team.ID",
                principalTable: "Teams",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_PuzzleUsers_User.ID",
                table: "TeamMembers",
                column: "User.ID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Events_EventID",
                table: "Teams",
                column: "EventID",
                principalTable: "Events",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
