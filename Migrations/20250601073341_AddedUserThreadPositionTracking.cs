using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EF_Messages.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserThreadPositionTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThreadToMessage_Messages_MessageId",
                table: "ThreadToMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_ThreadToMessage_Threads_ThreadId",
                table: "ThreadToMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ThreadToMessage",
                table: "ThreadToMessage");

            migrationBuilder.RenameTable(
                name: "ThreadToMessage",
                newName: "ThreadToMessages");

            migrationBuilder.RenameColumn(
                name: "position",
                table: "ThreadToMessages",
                newName: "Position");

            migrationBuilder.RenameIndex(
                name: "IX_ThreadToMessage_MessageId",
                table: "ThreadToMessages",
                newName: "IX_ThreadToMessages_MessageId");

            migrationBuilder.AddColumn<int>(
                name: "LastReadPosition",
                table: "ThreadToUser",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ThreadToMessages",
                table: "ThreadToMessages",
                columns: new[] { "ThreadId", "MessageId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ThreadToMessages_Messages_MessageId",
                table: "ThreadToMessages",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "MessageId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ThreadToMessages_Threads_ThreadId",
                table: "ThreadToMessages",
                column: "ThreadId",
                principalTable: "Threads",
                principalColumn: "ThreadId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThreadToMessages_Messages_MessageId",
                table: "ThreadToMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ThreadToMessages_Threads_ThreadId",
                table: "ThreadToMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ThreadToMessages",
                table: "ThreadToMessages");

            migrationBuilder.DropColumn(
                name: "LastReadPosition",
                table: "ThreadToUser");

            migrationBuilder.RenameTable(
                name: "ThreadToMessages",
                newName: "ThreadToMessage");

            migrationBuilder.RenameColumn(
                name: "Position",
                table: "ThreadToMessage",
                newName: "position");

            migrationBuilder.RenameIndex(
                name: "IX_ThreadToMessages_MessageId",
                table: "ThreadToMessage",
                newName: "IX_ThreadToMessage_MessageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ThreadToMessage",
                table: "ThreadToMessage",
                columns: new[] { "ThreadId", "MessageId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ThreadToMessage_Messages_MessageId",
                table: "ThreadToMessage",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "MessageId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ThreadToMessage_Threads_ThreadId",
                table: "ThreadToMessage",
                column: "ThreadId",
                principalTable: "Threads",
                principalColumn: "ThreadId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
