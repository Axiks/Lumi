using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vanilla.TelegramBot.Migrations
{
    /// <inheritdoc />
    public partial class RemovedTgUrkfieldfromphotomodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TgUrl",
                table: "ImagesEntity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TgUrl",
                table: "ImagesEntity",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
