using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class HintCoinsForHelpThread : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CostForHelpThread",
                table: "Puzzles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DefaultCostForHelpThread",
                table: "Events",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
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
