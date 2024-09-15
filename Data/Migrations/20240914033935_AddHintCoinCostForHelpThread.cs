using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHintCoinCostForHelpThread : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CostForHelpThread",
                table: "Puzzles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultCostForHelpThread",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostForHelpThread",
                table: "Puzzles");

            migrationBuilder.DropColumn(
                name: "DefaultCostForHelpThread",
                table: "Events");
        }
    }
}
