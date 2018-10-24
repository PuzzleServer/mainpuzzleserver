using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class PuzzleLockoutState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SubmissionText",
                table: "Submissions",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailOnlyMode",
                table: "PuzzleStatePerTeam",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "LockoutStage",
                table: "PuzzleStatePerTeam",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutTime",
                table: "PuzzleStatePerTeam",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LockoutDurationMultiplier",
                table: "Events",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LockoutForgivenessTime",
                table: "Events",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "LockoutSpamCount",
                table: "Events",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<double>(
                name: "LockoutSpamDuration",
                table: "Events",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "MaxSubmissionCount",
                table: "Events",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEmailOnlyMode",
                table: "PuzzleStatePerTeam");

            migrationBuilder.DropColumn(
                name: "LockoutStage",
                table: "PuzzleStatePerTeam");

            migrationBuilder.DropColumn(
                name: "LockoutTime",
                table: "PuzzleStatePerTeam");

            migrationBuilder.DropColumn(
                name: "LockoutDurationMultiplier",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "LockoutForgivenessTime",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "LockoutSpamCount",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "LockoutSpamDuration",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "MaxSubmissionCount",
                table: "Events");

            migrationBuilder.AlterColumn<string>(
                name: "SubmissionText",
                table: "Submissions",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
