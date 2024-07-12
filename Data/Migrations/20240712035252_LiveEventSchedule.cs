using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class LiveEventSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LiveEvents",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssociatedPuzzleId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventEndTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventStartTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventIsScheduled = table.Column<bool>(type: "bit", nullable: false),
                    NumberOfInstances = table.Column<int>(type: "int", nullable: false),
                    TimePerSlot = table.Column<TimeSpan>(type: "time", nullable: false),
                    TeamsPerSlot = table.Column<int>(type: "int", nullable: false),
                    OpeningReminderOffset = table.Column<TimeSpan>(type: "time", nullable: false),
                    ClosingReminderOffset = table.Column<TimeSpan>(type: "time", nullable: false),
                    FirstReminderOffset = table.Column<TimeSpan>(type: "time", nullable: false),
                    LastReminderOffset = table.Column<TimeSpan>(type: "time", nullable: false),
                    LastNotifiedAllTeamsUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveEvents", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LiveEvents_Puzzles_AssociatedPuzzleId",
                        column: x => x.AssociatedPuzzleId,
                        principalTable: "Puzzles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiveEventsSchedule",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LiveEventId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    StartTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastNotifiedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveEventsSchedule", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LiveEventsSchedule_LiveEvents_LiveEventId",
                        column: x => x.LiveEventId,
                        principalTable: "LiveEvents",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveEventsSchedule_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LiveEvents_AssociatedPuzzleId",
                table: "LiveEvents",
                column: "AssociatedPuzzleId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEventsSchedule_LiveEventId",
                table: "LiveEventsSchedule",
                column: "LiveEventId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveEventsSchedule_TeamId",
                table: "LiveEventsSchedule",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LiveEventsSchedule");

            migrationBuilder.DropTable(
                name: "LiveEvents");
        }
    }
}
