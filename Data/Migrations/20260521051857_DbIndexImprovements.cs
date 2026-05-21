using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class DbIndexImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamMembers_Team.ID",
                table: "TeamMembers");

            migrationBuilder.DropIndex(
                name: "IX_Swag_PlayerId",
                table: "Swag");

            migrationBuilder.DropIndex(
                name: "IX_Pieces_ProgressLevel",
                table: "Pieces");

            migrationBuilder.DropIndex(
                name: "IX_EventAuthors_AuthorID",
                table: "EventAuthors");

            migrationBuilder.DropIndex(
                name: "IX_EventAdmins_AdminID",
                table: "EventAdmins");

            migrationBuilder.AlterColumn<string>(
                name: "IdentityUserId",
                table: "PuzzleUsers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_Team.ID_User.ID",
                table: "TeamMembers",
                columns: new[] { "Team.ID", "User.ID" })
                .Annotation("SqlServer:Include", new[] { "ID", "ClassID", "TemporaryClassID" });

            migrationBuilder.CreateIndex(
                name: "IX_Swag_PlayerId_EventId",
                table: "Swag",
                columns: new[] { "PlayerId", "EventId" });

            migrationBuilder.CreateIndex(
                name: "IX_PuzzleUsers_IdentityUserId",
                table: "PuzzleUsers",
                column: "IdentityUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Puzzles_MinutesOfEventLockout_Availability_ID",
                table: "Puzzles",
                columns: new[] { "MinutesOfEventLockout", "Availability", "ID" });

            migrationBuilder.CreateIndex(
                name: "IX_Puzzles_MinutesToAutomaticallySolve_Availability_ID",
                table: "Puzzles",
                columns: new[] { "MinutesToAutomaticallySolve", "Availability", "ID" });

            migrationBuilder.CreateIndex(
                name: "IX_Pieces_ProgressLevel_PuzzleID",
                table: "Pieces",
                columns: new[] { "ProgressLevel", "PuzzleID" })
                .Annotation("SqlServer:Include", new[] { "ID", "Contents" });

            migrationBuilder.CreateIndex(
                name: "IX_EventAuthors_AuthorID_EventID",
                table: "EventAuthors",
                columns: new[] { "AuthorID", "EventID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventAdmins_AdminID_EventID",
                table: "EventAdmins",
                columns: new[] { "AdminID", "EventID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentFiles_EventID_FileType",
                table: "ContentFiles",
                columns: new[] { "EventID", "FileType" })
                .Annotation("SqlServer:Include", new[] { "ShortName", "UrlString", "PuzzleID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamMembers_Team.ID_User.ID",
                table: "TeamMembers");

            migrationBuilder.DropIndex(
                name: "IX_Swag_PlayerId_EventId",
                table: "Swag");

            migrationBuilder.DropIndex(
                name: "IX_PuzzleUsers_IdentityUserId",
                table: "PuzzleUsers");

            migrationBuilder.DropIndex(
                name: "IX_Puzzles_MinutesOfEventLockout_Availability_ID",
                table: "Puzzles");

            migrationBuilder.DropIndex(
                name: "IX_Puzzles_MinutesToAutomaticallySolve_Availability_ID",
                table: "Puzzles");

            migrationBuilder.DropIndex(
                name: "IX_Pieces_ProgressLevel_PuzzleID",
                table: "Pieces");

            migrationBuilder.DropIndex(
                name: "IX_EventAuthors_AuthorID_EventID",
                table: "EventAuthors");

            migrationBuilder.DropIndex(
                name: "IX_EventAdmins_AdminID_EventID",
                table: "EventAdmins");

            migrationBuilder.DropIndex(
                name: "IX_ContentFiles_EventID_FileType",
                table: "ContentFiles");

            migrationBuilder.AlterColumn<string>(
                name: "IdentityUserId",
                table: "PuzzleUsers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_Team.ID",
                table: "TeamMembers",
                column: "Team.ID");

            migrationBuilder.CreateIndex(
                name: "IX_Swag_PlayerId",
                table: "Swag",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Pieces_ProgressLevel",
                table: "Pieces",
                column: "ProgressLevel");

            migrationBuilder.CreateIndex(
                name: "IX_EventAuthors_AuthorID",
                table: "EventAuthors",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_EventAdmins_AdminID",
                table: "EventAdmins",
                column: "AdminID");
        }
    }
}
