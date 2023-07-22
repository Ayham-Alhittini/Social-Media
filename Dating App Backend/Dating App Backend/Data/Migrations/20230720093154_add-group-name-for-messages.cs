using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dating_App_Backend.Migrations
{
    /// <inheritdoc />
    public partial class addgroupnameformessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GroupName",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupName",
                table: "Messages");
        }
    }
}
