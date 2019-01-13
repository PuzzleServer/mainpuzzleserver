using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class RenameEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailAddress",
                table: "PuzzleUsers",
                newName: "Email");

            migrationBuilder.AlterColumn<string>(
                name: "IdentityUserId",
                table: "PuzzleUsers",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "PuzzleUsers",
                newName: "EmailAddress");

            migrationBuilder.AlterColumn<string>(
                name: "IdentityUserId",
                table: "PuzzleUsers",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
