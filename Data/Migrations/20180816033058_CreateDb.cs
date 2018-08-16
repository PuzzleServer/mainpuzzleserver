using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Data.Migrations
{
    public partial class CreateDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "States",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsSolved = table.Column<bool>(nullable: false),
                    IsUnlocked = table.Column<bool>(nullable: false),
                    Notes = table.Column<string>(nullable: true),
                    Printed = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CustomRoom = table.Column<string>(nullable: true),
                    EventID = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    PrimaryContactEmail = table.Column<string>(nullable: true),
                    PrimaryPhoneNumber = table.Column<string>(nullable: true),
                    PuzzleCacheLastUpdated = table.Column<DateTime>(nullable: false),
                    RoomID = table.Column<int>(nullable: false),
                    SecondaryPhoneNumber = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EmailAddress = table.Column<string>(nullable: true),
                    EmployeeAlias = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    TShirtSize = table.Column<string>(nullable: true),
                    VisibleToOthers = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Invitations",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EmailAddress = table.Column<string>(nullable: true),
                    Expiration = table.Column<DateTime>(nullable: false),
                    InvitationCode = table.Column<Guid>(nullable: false),
                    InvitationType = table.Column<string>(nullable: true),
                    TeamID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invitations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Invitations_Teams_TeamID",
                        column: x => x.TeamID,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamMembers",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TeamID = table.Column<int>(name: "Team.ID", nullable: true),
                    UserID = table.Column<int>(name: "User.ID", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMembers", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Teams_Team.ID",
                        column: x => x.TeamID,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Users_User.ID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventAuthors",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EventID = table.Column<int>(name: "Event.ID", nullable: true),
                    UserID = table.Column<int>(name: "User.ID", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventAuthors", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EventAuthors_Users_User.ID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventOwners",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EventID = table.Column<int>(name: "Event.ID", nullable: true),
                    UserID = table.Column<int>(name: "User.ID", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventOwners", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EventOwners_Users_User.ID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventAdmins",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EventID = table.Column<int>(name: "Event.ID", nullable: true),
                    UserID = table.Column<int>(name: "User.ID", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventAdmins", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EventAdmins_EventOwners_Event.ID",
                        column: x => x.EventID,
                        principalTable: "EventOwners",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventAdmins_Users_User.ID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Event",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AdminsID = table.Column<int>(nullable: true),
                    AllowFeedback = table.Column<bool>(nullable: false),
                    AnswerSubmissionEnd = table.Column<DateTime>(nullable: false),
                    AnswersAvailableBegin = table.Column<DateTime>(nullable: false),
                    EventBegin = table.Column<DateTime>(nullable: false),
                    IsInternEvent = table.Column<bool>(nullable: false),
                    MaxExternalsPerTeam = table.Column<int>(nullable: false),
                    MaxNumberOfTeams = table.Column<int>(nullable: false),
                    MaxTeamSize = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ShowFastestSolves = table.Column<bool>(nullable: false),
                    StandingsAvailableBegin = table.Column<DateTime>(nullable: false),
                    StandingsOverride = table.Column<bool>(nullable: false),
                    TeamDeleteEnd = table.Column<DateTime>(nullable: false),
                    TeamMembershipChangeEnd = table.Column<DateTime>(nullable: false),
                    TeamMiscDataChangeEnd = table.Column<DateTime>(nullable: false),
                    TeamNameChangeEnd = table.Column<DateTime>(nullable: false),
                    TeamRegistrationBegin = table.Column<DateTime>(nullable: false),
                    TeamRegistrationEnd = table.Column<DateTime>(nullable: false),
                    UrlString = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Event_EventAdmins_AdminsID",
                        column: x => x.AdminsID,
                        principalTable: "EventAdmins",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

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
                        name: "FK_EventTeams_Event_Event.ID",
                        column: x => x.EventID,
                        principalTable: "Event",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventTeams_Teams_Teams.ID",
                        column: x => x.TeamsID,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Puzzle",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AnswerUrlString = table.Column<string>(nullable: true),
                    EventID = table.Column<int>(nullable: true),
                    FirstSolveValue = table.Column<int>(nullable: false),
                    IsFinalPuzzle = table.Column<bool>(nullable: false),
                    IsMetaPuzzle = table.Column<bool>(nullable: false),
                    IsPuzzle = table.Column<bool>(nullable: false),
                    MaterialsUrlString = table.Column<string>(nullable: true),
                    MinValue = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    PerSolvePenalty = table.Column<int>(nullable: false),
                    PuzzleUrlString = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Puzzle", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Puzzle_Event_EventID",
                        column: x => x.EventID,
                        principalTable: "Event",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Difficulty = table.Column<int>(nullable: false),
                    Fun = table.Column<int>(nullable: false),
                    PuzzleID = table.Column<int>(nullable: true),
                    SubmissionTime = table.Column<DateTime>(nullable: false),
                    SubmitterID = table.Column<int>(nullable: true),
                    WrittenFeedback = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Feedback_Puzzle_PuzzleID",
                        column: x => x.PuzzleID,
                        principalTable: "Puzzle",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedback_Users_SubmitterID",
                        column: x => x.SubmitterID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PuzzleAuthors",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PuzzleID = table.Column<int>(name: "Puzzle.ID", nullable: true),
                    UserID = table.Column<int>(name: "User.ID", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuzzleAuthors", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PuzzleAuthors_Puzzle_Puzzle.ID",
                        column: x => x.PuzzleID,
                        principalTable: "Puzzle",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PuzzleAuthors_Users_User.ID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PuzzleStatePerTeam",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PuzzleID = table.Column<int>(nullable: true),
                    StateChanged = table.Column<DateTime>(nullable: false),
                    StateID = table.Column<int>(nullable: true),
                    TeamID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuzzleStatePerTeam", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PuzzleStatePerTeam_Puzzle_PuzzleID",
                        column: x => x.PuzzleID,
                        principalTable: "Puzzle",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PuzzleStatePerTeam_States_StateID",
                        column: x => x.StateID,
                        principalTable: "States",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PuzzleStatePerTeam_Teams_TeamID",
                        column: x => x.TeamID,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Responses",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsSolution = table.Column<bool>(nullable: false),
                    Note = table.Column<string>(nullable: true),
                    PuzzleID = table.Column<int>(nullable: true),
                    SubmittedText = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responses", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Responses_Puzzle_PuzzleID",
                        column: x => x.PuzzleID,
                        principalTable: "Puzzle",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PuzzleID = table.Column<int>(nullable: true),
                    ResponseID = table.Column<int>(nullable: true),
                    SubmissionText = table.Column<string>(nullable: true),
                    SubmitterID = table.Column<int>(nullable: true),
                    TeamID = table.Column<int>(nullable: true),
                    TimeSubmitted = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Submissions_Puzzle_PuzzleID",
                        column: x => x.PuzzleID,
                        principalTable: "Puzzle",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Submissions_Responses_ResponseID",
                        column: x => x.ResponseID,
                        principalTable: "Responses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Submissions_Users_SubmitterID",
                        column: x => x.SubmitterID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Submissions_Teams_TeamID",
                        column: x => x.TeamID,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Event_AdminsID",
                table: "Event",
                column: "AdminsID");

            migrationBuilder.CreateIndex(
                name: "IX_EventAdmins_Event.ID",
                table: "EventAdmins",
                column: "Event.ID");

            migrationBuilder.CreateIndex(
                name: "IX_EventAdmins_User.ID",
                table: "EventAdmins",
                column: "User.ID");

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
                name: "IX_EventOwners_Event.ID",
                table: "EventOwners",
                column: "Event.ID");

            migrationBuilder.CreateIndex(
                name: "IX_EventOwners_User.ID",
                table: "EventOwners",
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

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_PuzzleID",
                table: "Feedback",
                column: "PuzzleID");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_SubmitterID",
                table: "Feedback",
                column: "SubmitterID");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_TeamID",
                table: "Invitations",
                column: "TeamID");

            migrationBuilder.CreateIndex(
                name: "IX_Puzzle_EventID",
                table: "Puzzle",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_PuzzleAuthors_Puzzle.ID",
                table: "PuzzleAuthors",
                column: "Puzzle.ID");

            migrationBuilder.CreateIndex(
                name: "IX_PuzzleAuthors_User.ID",
                table: "PuzzleAuthors",
                column: "User.ID");

            migrationBuilder.CreateIndex(
                name: "IX_PuzzleStatePerTeam_PuzzleID",
                table: "PuzzleStatePerTeam",
                column: "PuzzleID");

            migrationBuilder.CreateIndex(
                name: "IX_PuzzleStatePerTeam_StateID",
                table: "PuzzleStatePerTeam",
                column: "StateID");

            migrationBuilder.CreateIndex(
                name: "IX_PuzzleStatePerTeam_TeamID",
                table: "PuzzleStatePerTeam",
                column: "TeamID");

            migrationBuilder.CreateIndex(
                name: "IX_Responses_PuzzleID",
                table: "Responses",
                column: "PuzzleID");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_PuzzleID",
                table: "Submissions",
                column: "PuzzleID");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ResponseID",
                table: "Submissions",
                column: "ResponseID");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_SubmitterID",
                table: "Submissions",
                column: "SubmitterID");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_TeamID",
                table: "Submissions",
                column: "TeamID");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_Team.ID",
                table: "TeamMembers",
                column: "Team.ID",
                unique: true,
                filter: "[Team.ID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_User.ID",
                table: "TeamMembers",
                column: "User.ID");

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Event_EventAdmins_AdminsID",
                table: "Event");

            migrationBuilder.DropTable(
                name: "EventAuthors");

            migrationBuilder.DropTable(
                name: "EventTeams");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropTable(
                name: "Invitations");

            migrationBuilder.DropTable(
                name: "PuzzleAuthors");

            migrationBuilder.DropTable(
                name: "PuzzleStatePerTeam");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropTable(
                name: "TeamMembers");

            migrationBuilder.DropTable(
                name: "States");

            migrationBuilder.DropTable(
                name: "Responses");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Puzzle");

            migrationBuilder.DropTable(
                name: "EventAdmins");

            migrationBuilder.DropTable(
                name: "EventOwners");

            migrationBuilder.DropTable(
                name: "Event");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
