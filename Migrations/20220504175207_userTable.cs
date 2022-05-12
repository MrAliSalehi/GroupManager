using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupManager.Migrations
{
    public partial class userTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Warns = table.Column<short>(type: "INTEGER", nullable: false),
                    IsBanned = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsBot = table.Column<bool>(type: "INTEGER", nullable: false),
                    StickerLimits = table.Column<short>(type: "INTEGER", nullable: false),
                    SentStickers = table.Column<short>(type: "INTEGER", nullable: false),
                    VideoLimits = table.Column<short>(type: "INTEGER", nullable: false),
                    SentVideos = table.Column<short>(type: "INTEGER", nullable: false),
                    GifLimits = table.Column<short>(type: "INTEGER", nullable: false),
                    SentGif = table.Column<short>(type: "INTEGER", nullable: false),
                    PhotoLimits = table.Column<short>(type: "INTEGER", nullable: false),
                    SentPhotos = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
