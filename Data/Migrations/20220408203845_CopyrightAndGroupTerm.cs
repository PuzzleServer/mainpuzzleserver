using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class CopyrightAndGroupTerm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Copyright",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TermForGroup",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Copyright",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "TermForGroup",
                table: "Events");
        }
    }
}
