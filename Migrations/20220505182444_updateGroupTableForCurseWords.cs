using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupManager.Migrations
{
    public partial class updateGroupTableForCurseWords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BanOnCurse",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MuteOnCurse",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "MuteTime",
                table: "Groups",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "WarnOnCurse",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BanOnCurse",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "MuteOnCurse",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "MuteTime",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "WarnOnCurse",
                table: "Groups");
        }
    }
}
