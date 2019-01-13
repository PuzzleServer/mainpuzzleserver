using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class RemoveEventOwners : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventAdmins_EventOwners_Event.ID",
                table: "EventAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_EventAdmins_AdminsID",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Prerequisites_Puzzles_PrerequisiteID",
                table: "Prerequisites");

            migrationBuilder.DropForeignKey(
                name: "FK_Prerequisites_Puzzles_PuzzleID",
                table: "Prerequisites");

            migrationBuilder.DropTable(
                name: "EventOwners");

            migrationBuilder.DropIndex(
                name: "IX_Events_AdminsID",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_EventAdmins_Event.ID",
                table: "EventAdmins");

            migrationBuilder.DropColumn(
                name: "AdminsID",
                table: "Events");

            migrationBuilder.AlterColumn<int>(
                name: "PuzzleID",
                table: "Prerequisites",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PrerequisiteID",
                table: "Prerequisites",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventAdmins_Event.ID",
                table: "EventAdmins",
                column: "Event.ID",
                unique: true,
                filter: "[Event.ID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_EventAdmins_Events_Event.ID",
                table: "EventAdmins",
                column: "Event.ID",
                principalTable: "Events",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prerequisites_Puzzles_PrerequisiteID",
                table: "Prerequisites",
                column: "PrerequisiteID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prerequisites_Puzzles_PuzzleID",
                table: "Prerequisites",
                column: "PuzzleID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventAdmins_Events_Event.ID",
                table: "EventAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_Prerequisites_Puzzles_PrerequisiteID",
                table: "Prerequisites");

            migrationBuilder.DropForeignKey(
                name: "FK_Prerequisites_Puzzles_PuzzleID",
                table: "Prerequisites");

            migrationBuilder.DropIndex(
                name: "IX_EventAdmins_Event.ID",
                table: "EventAdmins");

            migrationBuilder.AlterColumn<int>(
                name: "PuzzleID",
                table: "Prerequisites",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "PrerequisiteID",
                table: "Prerequisites",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "AdminsID",
                table: "Events",
                nullable: true);

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
                        name: "FK_EventOwners_Events_Event.ID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventOwners_PuzzleUsers_User.ID",
                        column: x => x.UserID,
                        principalTable: "PuzzleUsers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_AdminsID",
                table: "Events",
                column: "AdminsID");

            migrationBuilder.CreateIndex(
                name: "IX_EventAdmins_Event.ID",
                table: "EventAdmins",
                column: "Event.ID");

            migrationBuilder.CreateIndex(
                name: "IX_EventOwners_Event.ID",
                table: "EventOwners",
                column: "Event.ID");

            migrationBuilder.CreateIndex(
                name: "IX_EventOwners_User.ID",
                table: "EventOwners",
                column: "User.ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EventAdmins_EventOwners_Event.ID",
                table: "EventAdmins",
                column: "Event.ID",
                principalTable: "EventOwners",
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
                name: "FK_Prerequisites_Puzzles_PrerequisiteID",
                table: "Prerequisites",
                column: "PrerequisiteID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prerequisites_Puzzles_PuzzleID",
                table: "Prerequisites",
                column: "PuzzleID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
