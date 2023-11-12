using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dating_App_Backend.Migrations
{
    /// <inheritdoc />
    public partial class EditChatGroupRelWithHisPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GroupPhotoId",
                table: "ChatGroups",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_ChatGroups_GroupPhotoId",
                table: "ChatGroups",
                column: "GroupPhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatGroups_Files_GroupPhotoId",
                table: "ChatGroups",
                column: "GroupPhotoId",
                principalTable: "Files",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatGroups_Files_GroupPhotoId",
                table: "ChatGroups");

            migrationBuilder.DropIndex(
                name: "IX_ChatGroups_GroupPhotoId",
                table: "ChatGroups");

            migrationBuilder.AlterColumn<int>(
                name: "GroupPhotoId",
                table: "ChatGroups",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
