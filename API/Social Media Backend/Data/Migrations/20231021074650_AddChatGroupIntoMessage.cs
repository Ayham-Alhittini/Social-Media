using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dating_App_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddChatGroupIntoMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChatGroupId",
                table: "Messages",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGroupMessage",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatGroupId",
                table: "Messages",
                column: "ChatGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ChatGroups_ChatGroupId",
                table: "Messages",
                column: "ChatGroupId",
                principalTable: "ChatGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ChatGroups_ChatGroupId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ChatGroupId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ChatGroupId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsGroupMessage",
                table: "Messages");
        }
    }
}
