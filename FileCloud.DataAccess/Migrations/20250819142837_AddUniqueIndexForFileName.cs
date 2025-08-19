using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileCloud.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexForFileName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Files_FolderId",
                table: "Files");

            migrationBuilder.CreateIndex(
                name: "IX_Files_FolderId_Name",
                table: "Files",
                columns: new[] { "FolderId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Files_FolderId_Name",
                table: "Files");

            migrationBuilder.CreateIndex(
                name: "IX_Files_FolderId",
                table: "Files",
                column: "FolderId");
        }
    }
}
