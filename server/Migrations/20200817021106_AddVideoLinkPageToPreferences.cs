using Microsoft.EntityFrameworkCore.Migrations;

namespace TobyBlazor.Migrations
{
    public partial class AddVideoLinkPageToPreferences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentVideoPageLinkPage",
                table: "Preferences",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentVideoPageLinkPage",
                table: "Preferences");
        }
    }
}
