using Microsoft.EntityFrameworkCore.Migrations;

namespace TobyBlazor.Migrations
{
    public partial class ImConfusedAboutMyOwnCodeWTF : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CurrentRecentlyPlayedPageLinkPage",
                table: "Preferences",
                newName: "CurrentRecentlyPlayedVideoPage");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CurrentRecentlyPlayedVideoPage",
                table: "Preferences",
                newName: "CurrentRecentlyPlayedPageLinkPage");
        }
    }
}
