using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateNewPuzzleMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeneralMessages",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThreadId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventID = table.Column<int>(type: "int", nullable: false),
                    DateTimeInUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderID = table.Column<int>(type: "int", nullable: false),
                    IsMarkedAsRead = table.Column<bool>(type: "bit", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PuzzleID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralMessages", x => x.ID);
                    table.ForeignKey(
                        name: "FK_GeneralMessages_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GeneralMessages_PuzzleUsers_SenderID",
                        column: x => x.SenderID,
                        principalTable: "PuzzleUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeneralMessages_Puzzles_PuzzleID",
                        column: x => x.PuzzleID,
                        principalTable: "Puzzles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneralMessages_EventID",
                table: "GeneralMessages",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralMessages_PuzzleID",
                table: "GeneralMessages",
                column: "PuzzleID");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralMessages_SenderID",
                table: "GeneralMessages",
                column: "SenderID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneralMessages");
        }
    }
}
