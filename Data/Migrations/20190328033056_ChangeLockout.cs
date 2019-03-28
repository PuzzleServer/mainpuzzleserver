using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class ChangeLockout : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LockoutIncorrectGuessPeriod",
                table: "Events",
                newName: "LockoutBaseDuration");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LockoutBaseDuration",
                table: "Events",
                newName: "LockoutIncorrectGuessPeriod");
        }
    }
}
