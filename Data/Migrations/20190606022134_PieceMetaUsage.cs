using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class PieceMetaUsage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PieceMetaUsage",
                table: "Puzzles",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PieceMetaUsage",
                table: "Puzzles");
        }
    }
}
