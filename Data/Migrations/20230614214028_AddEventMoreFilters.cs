using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEventMoreFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EventIsRemote",
                table: "Events",
                newName: "IsRemote");

            migrationBuilder.RenameColumn(
                name: "EventHasSwag",
                table: "Events",
                newName: "HasTShirts");

            migrationBuilder.RenameColumn(
                name: "EventAllowsRemote",
                table: "Events",
                newName: "HasSwag");

            migrationBuilder.AddColumn<string>(
                name: "IsRemote",
                table: "PlayerInEvent",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowsRemote",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasIndividualLunch",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRemote",
                table: "PlayerInEvent");

            migrationBuilder.DropColumn(
                name: "AllowsRemote",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "HasIndividualLunch",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "IsRemote",
                table: "Events",
                newName: "EventIsRemote");

            migrationBuilder.RenameColumn(
                name: "HasTShirts",
                table: "Events",
                newName: "EventHasSwag");

            migrationBuilder.RenameColumn(
                name: "HasSwag",
                table: "Events",
                newName: "EventAllowsRemote");
        }
    }
}
