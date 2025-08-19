using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileCloud.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexForFolderName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Folders_ParentId",
                table: "Folders");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ParentId_Name",
                table: "Folders",
                columns: new[] { "ParentId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Folders_ParentId_Name",
                table: "Folders");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ParentId",
                table: "Folders",
                column: "ParentId");
        }
    }
}
