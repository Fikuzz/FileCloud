using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileCloud.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RootFolder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Folders",
                columns: new[] { "Id", "Name", "ParentId" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "Root", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Folders",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));
        }
    }
}
