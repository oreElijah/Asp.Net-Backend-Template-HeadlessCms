using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeadlessCms.Migrations
{
    /// <inheritdoc />
    public partial class AddAppUserIdToContentEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "ContentEntry",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentEntry_AppUserId",
                table: "ContentEntry",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentEntry_AspNetUsers_AppUserId",
                table: "ContentEntry",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentEntry_AspNetUsers_AppUserId",
                table: "ContentEntry");

            migrationBuilder.DropIndex(
                name: "IX_ContentEntry_AppUserId",
                table: "ContentEntry");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "ContentEntry");
        }
    }
}
