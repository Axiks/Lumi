﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vanilla.TelegramBot.Migrations
{
    /// <inheritdoc />
    public partial class addeUserImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImageEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TgMediaId = table.Column<string>(type: "text", nullable: false),
                    TgUrl = table.Column<string>(type: "text", nullable: false),
                    UserEntityUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageEntity_Users_UserEntityUserId",
                        column: x => x.UserEntityUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageEntity_UserEntityUserId",
                table: "ImageEntity",
                column: "UserEntityUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageEntity");
        }
    }
}
