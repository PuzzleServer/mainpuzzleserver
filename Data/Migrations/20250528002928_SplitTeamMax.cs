using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitTeamMax : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsForLocalOnly",
                table: "Puzzles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsForRemoteOnly",
                table: "Puzzles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LockChangesToRemoteStatus",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxNumberOfLocalTeams",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxNumberOfRemoteTeams",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsForLocalOnly",
                table: "Puzzles");

            migrationBuilder.DropColumn(
                name: "IsForRemoteOnly",
                table: "Puzzles");

            migrationBuilder.DropColumn(
                name: "LockChangesToRemoteStatus",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "MaxNumberOfLocalTeams",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "MaxNumberOfRemoteTeams",
                table: "Events");
        }
    }
}
