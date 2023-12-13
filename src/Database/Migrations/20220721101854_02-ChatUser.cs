using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1062 // Validate Parameter is non-null before using it
namespace Database.Migrations
{
    public partial class _02ChatUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Chat_TableChatId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_TableChatId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "TableChatId",
                table: "User");

            migrationBuilder.AddColumn<byte[]>(
                name: "DataVersion",
                table: "IdeaReport",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "DataVersion",
                table: "IdeaOrganization",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "DataVersion",
                table: "IdeaLike",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "ChatUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    TblUserId = table.Column<long>(type: "bigint", nullable: false),
                    TblChatId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatUsers_Chat_TblChatId",
                        column: x => x.TblChatId,
                        principalTable: "Chat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatUsers_User_TblUserId",
                        column: x => x.TblUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatUsers_TblChatId",
                table: "ChatUsers",
                column: "TblChatId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatUsers_TblUserId",
                table: "ChatUsers",
                column: "TblUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatUsers");

            migrationBuilder.DropColumn(
                name: "DataVersion",
                table: "IdeaReport");

            migrationBuilder.DropColumn(
                name: "DataVersion",
                table: "IdeaOrganization");

            migrationBuilder.DropColumn(
                name: "DataVersion",
                table: "IdeaLike");

            migrationBuilder.AddColumn<long>(
                name: "TableChatId",
                table: "User",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_TableChatId",
                table: "User",
                column: "TableChatId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Chat_TableChatId",
                table: "User",
                column: "TableChatId",
                principalTable: "Chat",
                principalColumn: "Id");
        }
    }
}
