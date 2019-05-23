using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class BioAndPassword : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Teams",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Teams",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Teams");
        }
    }
}
