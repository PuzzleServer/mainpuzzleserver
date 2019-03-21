using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class HintsAreCumulative : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HintsAreCumulative",
                table: "Puzzles",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HintsAreCumulative",
                table: "Puzzles");
        }
    }
}
