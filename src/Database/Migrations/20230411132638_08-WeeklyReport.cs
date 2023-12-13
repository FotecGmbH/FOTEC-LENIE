using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1062 // Validate Parameter is non-null before using it

namespace Database.Migrations
{
    public partial class _08WeeklyReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AmountLabel",
                table: "IdeaNeed",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "IdeaLike",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountLabel",
                table: "IdeaNeed");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "IdeaLike");
        }
    }
}
