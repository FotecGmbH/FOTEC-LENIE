using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1062 // Validate Parameter is non-null before using it
namespace Database.Migrations
{
    public partial class _01AddressSettingPushSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotificationPushSupport",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LocationAddress",
                table: "Idea",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationPushSupport",
                table: "User");

            migrationBuilder.DropColumn(
                name: "LocationAddress",
                table: "Idea");
        }
    }
}
