using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class HideStandings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StandingsAvailableBegin",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "StandingsOverride",
                table: "Events",
                newName: "HideStandings");

            migrationBuilder.RenameColumn(
                name: "ShowFastestSolves",
                table: "Events",
                newName: "HideFastestSolves");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HideStandings",
                table: "Events",
                newName: "StandingsOverride");

            migrationBuilder.RenameColumn(
                name: "HideFastestSolves",
                table: "Events",
                newName: "ShowFastestSolves");

            migrationBuilder.AddColumn<DateTime>(
                name: "StandingsAvailableBegin",
                table: "Events",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
