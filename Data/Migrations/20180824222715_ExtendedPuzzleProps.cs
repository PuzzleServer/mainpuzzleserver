using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Data.Migrations
{
    public partial class ExtendedPuzzleProps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PerSolvePenalty",
                table: "Puzzle",
                newName: "SolveValue");

            migrationBuilder.RenameColumn(
                name: "MinValue",
                table: "Puzzle",
                newName: "OrderInGroup");

            migrationBuilder.RenameColumn(
                name: "FirstSolveValue",
                table: "Puzzle",
                newName: "MinPrerequisiteCount");

            migrationBuilder.AddColumn<string>(
                name: "Group",
                table: "Puzzle",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGloballyVisiblePrerequisite",
                table: "Puzzle",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "Puzzle",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Group",
                table: "Puzzle");

            migrationBuilder.DropColumn(
                name: "IsGloballyVisiblePrerequisite",
                table: "Puzzle");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "Puzzle");

            migrationBuilder.RenameColumn(
                name: "SolveValue",
                table: "Puzzle",
                newName: "PerSolvePenalty");

            migrationBuilder.RenameColumn(
                name: "OrderInGroup",
                table: "Puzzle",
                newName: "MinValue");

            migrationBuilder.RenameColumn(
                name: "MinPrerequisiteCount",
                table: "Puzzle",
                newName: "FirstSolveValue");
        }
    }
}
