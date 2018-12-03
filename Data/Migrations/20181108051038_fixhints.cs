using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class fixhints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Cost",
                table: "Hints",
                nullable: false,
                oldClrType: typeof(long));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Cost",
                table: "Hints",
                nullable: false,
                oldClrType: typeof(int));
        }
    }
}
