using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupManager.Migrations
{
    public partial class globalFix2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_ForceJoinChannels_Groups_GroupId",
            //    table: "ForceJoinChannels");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "MuteTime",
                table: "Groups",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "'03:00:00'",
                oldClrType: typeof(TimeSpan),
                oldType: "TEXT",
                oldDefaultValueSql: "'00:00:00'");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "ForceJoinChannels",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForceJoinChannels",
                table: "ForceJoinChannels",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ForceJoinChannels_Groups_GroupId",
                table: "ForceJoinChannels",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_ForceJoinChannels_Groups_GroupId",
            //    table: "ForceJoinChannels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ForceJoinChannels",
                table: "ForceJoinChannels");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ForceJoinChannels");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "MuteTime",
                table: "Groups",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "'00:00:00'",
                oldClrType: typeof(TimeSpan),
                oldType: "TEXT",
                oldDefaultValueSql: "'03:00:00'");

            migrationBuilder.AddForeignKey(
                name: "FK_ForceJoinChannels_Groups_GroupId",
                table: "ForceJoinChannels",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");
        }
    }
}
