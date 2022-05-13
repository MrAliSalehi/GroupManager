using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupManager.Migrations
{
    public partial class welcomeColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SayWelcome",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WelcomeMessage",
                table: "Groups",
                type: "TEXT",
                defaultValue: "Welcome To Group!",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SayWelcome",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "WelcomeMessage",
                table: "Groups");
        }
    }
}
