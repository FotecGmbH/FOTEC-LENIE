using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1062 // Validate Parameter is non-null before using it

namespace Database.Migrations
{
    public partial class _09_NugetUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Setting",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Permission",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Organization",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Intro",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "IdeaSupply",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "IdeaReport",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "IdeaOrganization",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "IdeaNeed",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "IdeaLike",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "IdeaHelper",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Idea",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "FutureWishLike",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "FutureWish",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "File",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Device",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "ChatUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "ChatEntry",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Chat",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "AccessToken",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "User");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Setting");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Organization");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Intro");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "IdeaSupply");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "IdeaReport");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "IdeaOrganization");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "IdeaNeed");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "IdeaLike");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "IdeaHelper");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Idea");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "FutureWishLike");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "FutureWish");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "File");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "ChatUsers");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "ChatEntry");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Chat");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "AccessToken");
        }
    }
}
