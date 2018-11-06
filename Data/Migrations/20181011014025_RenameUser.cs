using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class RenameUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventAdmins_Users_User.ID",
                table: "EventAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_EventAuthors_Users_User.ID",
                table: "EventAuthors");

            migrationBuilder.DropForeignKey(
                name: "FK_EventOwners_Users_User.ID",
                table: "EventOwners");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_Users_SubmitterID",
                table: "Feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_PuzzleAuthors_Users_User.ID",
                table: "PuzzleAuthors");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Users_SubmitterID",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_Users_User.ID",
                table: "TeamMembers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.CreateTable(
                name: "PuzzleUsers",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IdentityUserId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    EmployeeAlias = table.Column<string>(nullable: true),
                    EmailAddress = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    TShirtSize = table.Column<string>(nullable: true),
                    VisibleToOthers = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuzzleUsers", x => x.ID);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_EventAdmins_PuzzleUsers_User.ID",
                table: "EventAdmins",
                column: "User.ID",
                principalTable: "PuzzleUsers",
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
                name: "FK_EventOwners_PuzzleUsers_User.ID",
                table: "EventOwners",
                column: "User.ID",
                principalTable: "PuzzleUsers",
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
                name: "FK_PuzzleAuthors_PuzzleUsers_User.ID",
                table: "PuzzleAuthors",
                column: "User.ID",
                principalTable: "PuzzleUsers",
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
                name: "FK_TeamMembers_PuzzleUsers_User.ID",
                table: "TeamMembers",
                column: "User.ID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventAdmins_PuzzleUsers_User.ID",
                table: "EventAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_EventAuthors_PuzzleUsers_User.ID",
                table: "EventAuthors");

            migrationBuilder.DropForeignKey(
                name: "FK_EventOwners_PuzzleUsers_User.ID",
                table: "EventOwners");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_PuzzleUsers_SubmitterID",
                table: "Feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_PuzzleAuthors_PuzzleUsers_User.ID",
                table: "PuzzleAuthors");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_PuzzleUsers_SubmitterID",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_PuzzleUsers_User.ID",
                table: "TeamMembers");

            migrationBuilder.DropTable(
                name: "PuzzleUsers");

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

            migrationBuilder.AddForeignKey(
                name: "FK_EventAdmins_Users_User.ID",
                table: "EventAdmins",
                column: "User.ID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventAuthors_Users_User.ID",
                table: "EventAuthors",
                column: "User.ID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventOwners_Users_User.ID",
                table: "EventOwners",
                column: "User.ID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_Users_SubmitterID",
                table: "Feedback",
                column: "SubmitterID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PuzzleAuthors_Users_User.ID",
                table: "PuzzleAuthors",
                column: "User.ID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Users_SubmitterID",
                table: "Submissions",
                column: "SubmitterID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_Users_User.ID",
                table: "TeamMembers",
                column: "User.ID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
