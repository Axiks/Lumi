using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vanilla.TelegramBot.Migrations
{
    /// <inheritdoc />
    public partial class Addedaflagforwhethertheuserhascreatedaprofile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHasProfile",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHasProfile",
                table: "Users");
        }
    }
}
