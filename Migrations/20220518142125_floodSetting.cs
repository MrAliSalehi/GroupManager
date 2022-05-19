using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupManager.Migrations
{
    public partial class floodSetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RestrictTime",
                table: "FloodSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RestrictTime",
                table: "FloodSettings");
        }
    }
}
