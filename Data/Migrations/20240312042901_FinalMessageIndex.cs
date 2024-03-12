using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class FinalMessageIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_EventID_ThreadId",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_EventID",
                table: "Messages",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ThreadId",
                table: "Messages",
                column: "ThreadId",
                unique: true,
                filter: "[ThreadId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_EventID",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ThreadId",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_EventID_ThreadId",
                table: "Messages",
                columns: new[] { "EventID", "ThreadId" });
        }
    }
}
