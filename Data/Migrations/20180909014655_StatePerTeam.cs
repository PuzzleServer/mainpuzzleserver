using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class StatePerTeam : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PuzzleStatePerTeam_States_StateID",
                table: "PuzzleStatePerTeam");

            migrationBuilder.DropTable(
                name: "States");

            migrationBuilder.DropIndex(
                name: "IX_PuzzleStatePerTeam_StateID",
                table: "PuzzleStatePerTeam");

            migrationBuilder.DropColumn(
                name: "StateChanged",
                table: "PuzzleStatePerTeam");

            migrationBuilder.DropColumn(
                name: "StateID",
                table: "PuzzleStatePerTeam");

            migrationBuilder.AlterColumn<int>(
                name: "EventID",
                table: "Teams",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "PuzzleStatePerTeam",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Printed",
                table: "PuzzleStatePerTeam",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SolvedTime",
                table: "PuzzleStatePerTeam",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UnlockedTime",
                table: "PuzzleStatePerTeam",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_EventID",
                table: "Teams",
                column: "EventID");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Events_EventID",
                table: "Teams",
                column: "EventID",
                principalTable: "Events",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Events_EventID",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_EventID",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "PuzzleStatePerTeam");

            migrationBuilder.DropColumn(
                name: "Printed",
                table: "PuzzleStatePerTeam");

            migrationBuilder.DropColumn(
                name: "SolvedTime",
                table: "PuzzleStatePerTeam");

            migrationBuilder.DropColumn(
                name: "UnlockedTime",
                table: "PuzzleStatePerTeam");

            migrationBuilder.AlterColumn<int>(
                name: "EventID",
                table: "Teams",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StateChanged",
                table: "PuzzleStatePerTeam",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "StateID",
                table: "PuzzleStatePerTeam",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "States",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsSolved = table.Column<bool>(nullable: false),
                    IsUnlocked = table.Column<bool>(nullable: false),
                    Notes = table.Column<string>(nullable: true),
                    Printed = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PuzzleStatePerTeam_StateID",
                table: "PuzzleStatePerTeam",
                column: "StateID");

            migrationBuilder.AddForeignKey(
                name: "FK_PuzzleStatePerTeam_States_StateID",
                table: "PuzzleStatePerTeam",
                column: "StateID",
                principalTable: "States",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
