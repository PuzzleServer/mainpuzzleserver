using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsClaimedAndAddModified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsClaimed",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "DateTimeInUtc",
                table: "Messages",
                newName: "ModifiedDateTimeInUtc");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateTimeInUtc",
                table: "Messages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDateTimeInUtc",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "ModifiedDateTimeInUtc",
                table: "Messages",
                newName: "DateTimeInUtc");

            migrationBuilder.AddColumn<bool>(
                name: "IsClaimed",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
