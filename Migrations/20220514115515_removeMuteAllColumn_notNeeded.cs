using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupManager.Migrations
{
    public partial class removeMuteAllColumn_notNeeded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MuteAllChat",
                table: "Groups");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MuteAllChat",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
