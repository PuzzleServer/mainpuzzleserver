using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class SingleTableWithThreadIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeneralMessages_Events_EventID",
                table: "GeneralMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_GeneralMessages_PuzzleUsers_MarkReadUserID",
                table: "GeneralMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_GeneralMessages_PuzzleUsers_SenderID",
                table: "GeneralMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_GeneralMessages_Puzzles_PuzzleID",
                table: "GeneralMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_GeneralMessages_Teams_TeamID",
                table: "GeneralMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_GeneralMessages_Teams_TeamPuzzleMessage_TeamID",
                table: "GeneralMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GeneralMessages",
                table: "GeneralMessages");

            migrationBuilder.DropIndex(
                name: "IX_GeneralMessages_TeamPuzzleMessage_TeamID",
                table: "GeneralMessages");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "GeneralMessages");

            migrationBuilder.DropColumn(
                name: "TeamPuzzleMessage_TeamID",
                table: "GeneralMessages");

            migrationBuilder.RenameTable(
                name: "GeneralMessages",
                newName: "Messages");

            migrationBuilder.RenameColumn(
                name: "MarkReadUserID",
                table: "Messages",
                newName: "ClaimerID");

            migrationBuilder.RenameColumn(
                name: "IsMarkedAsRead",
                table: "Messages",
                newName: "IsClaimed");

            migrationBuilder.RenameIndex(
                name: "IX_GeneralMessages_TeamID",
                table: "Messages",
                newName: "IX_Messages_TeamID");

            migrationBuilder.RenameIndex(
                name: "IX_GeneralMessages_SenderID",
                table: "Messages",
                newName: "IX_Messages_SenderID");

            migrationBuilder.RenameIndex(
                name: "IX_GeneralMessages_PuzzleID",
                table: "Messages",
                newName: "IX_Messages_PuzzleID");

            migrationBuilder.RenameIndex(
                name: "IX_GeneralMessages_MarkReadUserID",
                table: "Messages",
                newName: "IX_Messages_ClaimerID");

            migrationBuilder.RenameIndex(
                name: "IX_GeneralMessages_EventID",
                table: "Messages",
                newName: "IX_Messages_EventID");

            migrationBuilder.AlterColumn<string>(
                name: "ThreadId",
                table: "Messages",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TeamID",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PuzzleID",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Messages",
                table: "Messages",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ThreadId",
                table: "Messages",
                column: "ThreadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Events_EventID",
                table: "Messages",
                column: "EventID",
                principalTable: "Events",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_PuzzleUsers_ClaimerID",
                table: "Messages",
                column: "ClaimerID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_PuzzleUsers_SenderID",
                table: "Messages",
                column: "SenderID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Puzzles_PuzzleID",
                table: "Messages",
                column: "PuzzleID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Teams_TeamID",
                table: "Messages",
                column: "TeamID",
                principalTable: "Teams",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Events_EventID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_PuzzleUsers_ClaimerID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_PuzzleUsers_SenderID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Puzzles_PuzzleID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Teams_TeamID",
                table: "Messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Messages",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ThreadId",
                table: "Messages");

            migrationBuilder.RenameTable(
                name: "Messages",
                newName: "GeneralMessages");

            migrationBuilder.RenameColumn(
                name: "IsClaimed",
                table: "GeneralMessages",
                newName: "IsMarkedAsRead");

            migrationBuilder.RenameColumn(
                name: "ClaimerID",
                table: "GeneralMessages",
                newName: "MarkReadUserID");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_TeamID",
                table: "GeneralMessages",
                newName: "IX_GeneralMessages_TeamID");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_SenderID",
                table: "GeneralMessages",
                newName: "IX_GeneralMessages_SenderID");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_PuzzleID",
                table: "GeneralMessages",
                newName: "IX_GeneralMessages_PuzzleID");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_EventID",
                table: "GeneralMessages",
                newName: "IX_GeneralMessages_EventID");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_ClaimerID",
                table: "GeneralMessages",
                newName: "IX_GeneralMessages_MarkReadUserID");

            migrationBuilder.AlterColumn<string>(
                name: "ThreadId",
                table: "GeneralMessages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TeamID",
                table: "GeneralMessages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PuzzleID",
                table: "GeneralMessages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "GeneralMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TeamPuzzleMessage_TeamID",
                table: "GeneralMessages",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GeneralMessages",
                table: "GeneralMessages",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralMessages_TeamPuzzleMessage_TeamID",
                table: "GeneralMessages",
                column: "TeamPuzzleMessage_TeamID");

            migrationBuilder.AddForeignKey(
                name: "FK_GeneralMessages_Events_EventID",
                table: "GeneralMessages",
                column: "EventID",
                principalTable: "Events",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GeneralMessages_PuzzleUsers_MarkReadUserID",
                table: "GeneralMessages",
                column: "MarkReadUserID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GeneralMessages_PuzzleUsers_SenderID",
                table: "GeneralMessages",
                column: "SenderID",
                principalTable: "PuzzleUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GeneralMessages_Puzzles_PuzzleID",
                table: "GeneralMessages",
                column: "PuzzleID",
                principalTable: "Puzzles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GeneralMessages_Teams_TeamID",
                table: "GeneralMessages",
                column: "TeamID",
                principalTable: "Teams",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GeneralMessages_Teams_TeamPuzzleMessage_TeamID",
                table: "GeneralMessages",
                column: "TeamPuzzleMessage_TeamID",
                principalTable: "Teams",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
