using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRemoteTeams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRemoteTeam",
                table: "Teams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SubmitterDisplayName",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmitterDisplayName",
                table: "SinglePlayerPuzzleSubmissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowsRemoteTeams",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EventPassword",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FastestSyncIntervalMs",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "PuzzleSyncEnabled",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRemoteTeam",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "SubmitterDisplayName",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "SubmitterDisplayName",
                table: "SinglePlayerPuzzleSubmissions");

            migrationBuilder.DropColumn(
                name: "AllowsRemoteTeams",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EventPassword",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "FastestSyncIntervalMs",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "PuzzleSyncEnabled",
                table: "Events");
        }
    }
}
