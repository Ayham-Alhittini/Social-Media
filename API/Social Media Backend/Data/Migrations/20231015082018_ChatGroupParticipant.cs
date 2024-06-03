using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dating_App_Backend.Migrations
{
    /// <inheritdoc />
    public partial class ChatGroupParticipant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatGroupParticipants",
                columns: table => new
                {
                    ChatGroupId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ParticipantId = table.Column<int>(type: "int", nullable: false),
                    ParticipantPhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParticipantUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParticipantKnownAs = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatGroupParticipants", x => new { x.ParticipantId, x.ChatGroupId });
                    table.ForeignKey(
                        name: "FK_ChatGroupParticipants_AspNetUsers_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatGroupParticipants_ChatGroups_ChatGroupId",
                        column: x => x.ChatGroupId,
                        principalTable: "ChatGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatGroupParticipants_ChatGroupId",
                table: "ChatGroupParticipants",
                column: "ChatGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatGroupParticipants");
        }
    }
}
