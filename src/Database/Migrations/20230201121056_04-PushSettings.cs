using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1062 // Validate Parameter is non-null before using it

namespace Database.Migrations
{
    public partial class _04PushSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotificationPushIdea",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotificationPushReport",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationPushIdea",
                table: "User");

            migrationBuilder.DropColumn(
                name: "NotificationPushReport",
                table: "User");
        }
    }
}
