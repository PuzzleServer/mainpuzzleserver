using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClassID",
                table: "TeamMembers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TemporaryClassID",
                table: "TeamMembers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasPlayerClasses",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PlayerClassName",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlayerClasses",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UniqueName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerClasses", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PlayerClasses_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_ClassID",
                table: "TeamMembers",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_TemporaryClassID",
                table: "TeamMembers",
                column: "TemporaryClassID");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerClasses_EventID",
                table: "PlayerClasses",
                column: "EventID");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_PlayerClasses_ClassID",
                table: "TeamMembers",
                column: "ClassID",
                principalTable: "PlayerClasses",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_PlayerClasses_TemporaryClassID",
                table: "TeamMembers",
                column: "TemporaryClassID",
                principalTable: "PlayerClasses",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_PlayerClasses_ClassID",
                table: "TeamMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_PlayerClasses_TemporaryClassID",
                table: "TeamMembers");

            migrationBuilder.DropTable(
                name: "PlayerClasses");

            migrationBuilder.DropIndex(
                name: "IX_TeamMembers_ClassID",
                table: "TeamMembers");

            migrationBuilder.DropIndex(
                name: "IX_TeamMembers_TemporaryClassID",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "ClassID",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "TemporaryClassID",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "HasPlayerClasses",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "PlayerClassName",
                table: "Events");
        }
    }
}
