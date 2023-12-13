using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1062 // Validate Parameter is non-null before using it
namespace Database.Migrations
{
    public partial class _03IdeaImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TblIdeaImageId",
                table: "Idea",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Idea_TblIdeaImageId",
                table: "Idea",
                column: "TblIdeaImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Idea_File_TblIdeaImageId",
                table: "Idea",
                column: "TblIdeaImageId",
                principalTable: "File",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Idea_File_TblIdeaImageId",
                table: "Idea");

            migrationBuilder.DropIndex(
                name: "IX_Idea_TblIdeaImageId",
                table: "Idea");

            migrationBuilder.DropColumn(
                name: "TblIdeaImageId",
                table: "Idea");
        }
    }
}
