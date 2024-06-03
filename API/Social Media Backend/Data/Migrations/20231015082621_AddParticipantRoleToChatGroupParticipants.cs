using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dating_App_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddParticipantRoleToChatGroupParticipants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParticipantRoleId",
                table: "ChatGroupParticipants",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatGroupParticipants_ParticipantRoleId",
                table: "ChatGroupParticipants",
                column: "ParticipantRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatGroupParticipants_ParticipantRoles_ParticipantRoleId",
                table: "ChatGroupParticipants",
                column: "ParticipantRoleId",
                principalTable: "ParticipantRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatGroupParticipants_ParticipantRoles_ParticipantRoleId",
                table: "ChatGroupParticipants");

            migrationBuilder.DropIndex(
                name: "IX_ChatGroupParticipants_ParticipantRoleId",
                table: "ChatGroupParticipants");

            migrationBuilder.DropColumn(
                name: "ParticipantRoleId",
                table: "ChatGroupParticipants");
        }
    }
}
