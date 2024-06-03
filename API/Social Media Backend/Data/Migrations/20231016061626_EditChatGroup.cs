using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dating_App_Backend.Migrations
{
    /// <inheritdoc />
    public partial class EditChatGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupPhotoUrl",
                table: "ChatGroups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GroupPhotoUrl",
                table: "ChatGroups",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
