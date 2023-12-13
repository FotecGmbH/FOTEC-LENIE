using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1062 // Validate Parameter is non-null before using it

namespace Database.Migrations
{
    public partial class _07TableIntro : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Archived",
                table: "FutureWish",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Intro",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Weblink = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Intro", x => x.Id);
                });

            migrationBuilder.Sql("INSERT INTO Intro (Weblink) VALUES ('https://dihost-lenie-connectivityhost.azurewebsites.net/Intro/One');");
            migrationBuilder.Sql("INSERT INTO Intro (Weblink) VALUES ('https://dihost-lenie-connectivityhost.azurewebsites.net/Intro/Two');");
            migrationBuilder.Sql("INSERT INTO Intro (Weblink) VALUES ('https://dihost-lenie-connectivityhost.azurewebsites.net/Intro/Three');");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Intro");

            migrationBuilder.DropColumn(
                name: "Archived",
                table: "FutureWish");
        }
    }
}
