using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupManager.Migrations
{
    public partial class limitationscolumnsForGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "GifLimits",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<bool>(
                name: "LimitMedia",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<uint>(
                name: "PhotoLimits",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "StickerLimits",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "VideoLimits",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GifLimits",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "LimitMedia",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "PhotoLimits",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "StickerLimits",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "VideoLimits",
                table: "Groups");
        }
    }
}
