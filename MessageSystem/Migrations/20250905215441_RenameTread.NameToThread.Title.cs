using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EF_Messages.Migrations
{
    /// <inheritdoc />
    public partial class RenameTreadNameToThreadTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Threads",
                newName: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Threads",
                newName: "Name");
        }
    }
}
