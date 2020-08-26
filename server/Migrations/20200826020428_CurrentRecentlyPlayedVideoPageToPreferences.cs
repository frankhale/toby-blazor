using Microsoft.EntityFrameworkCore.Migrations;

namespace TobyBlazor.Migrations
{
    public partial class CurrentRecentlyPlayedVideoPageToPreferences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentRecentlyPlayedVideoPage",
                table: "Preferences",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentRecentlyPlayedVideoPage",
                table: "Preferences");
        }
    }
}
