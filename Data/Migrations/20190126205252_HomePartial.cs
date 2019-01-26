using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class HomePartial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HomePartial",
                table: "Events",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HomePartial",
                table: "Events");
        }
    }
}
