using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class EventId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UrlString",
                table: "Events",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_UrlString",
                table: "Events",
                column: "UrlString",
                unique: true,
                filter: "[UrlString] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Events_UrlString",
                table: "Events");

            migrationBuilder.AlterColumn<string>(
                name: "UrlString",
                table: "Events",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
