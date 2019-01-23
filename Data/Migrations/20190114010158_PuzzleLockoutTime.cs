using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class PuzzleLockoutTime : Migration
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

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutExpiryTime",
                table: "PuzzleStatePerTeam",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "WrongSubmissionCountBuffer",
                table: "PuzzleStatePerTeam",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "SupportEmailAlias",
                table: "Puzzles",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LockoutDurationMultiplier",
                table: "Events",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "LockoutIncorrectGuessLimit",
                table: "Events",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "LockoutIncorrectGuessPeriod",
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
                name: "LockoutExpiryTime",
                table: "PuzzleStatePerTeam");

            migrationBuilder.DropColumn(
                name: "WrongSubmissionCountBuffer",
                table: "PuzzleStatePerTeam");

            migrationBuilder.DropColumn(
                name: "SupportEmailAlias",
                table: "Puzzles");

            migrationBuilder.DropColumn(
                name: "LockoutDurationMultiplier",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "LockoutIncorrectGuessLimit",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "LockoutIncorrectGuessPeriod",
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
