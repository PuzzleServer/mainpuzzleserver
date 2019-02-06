using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class HintCoinsUsed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HintCoinsUsed",
                table: "Teams",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HintCoinsUsed",
                table: "Teams");
        }
    }
}
