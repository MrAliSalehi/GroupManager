using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupManager.Migrations
{
    public partial class globalFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ForceJoinChannels",
                table: "ForceJoinChannels");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ForceJoinChannels");

            migrationBuilder.AlterColumn<string>(
                name: "WelcomeMessage",
                table: "Groups",
                type: "TEXT",
                nullable: true,
                defaultValueSql: "'Welcome To Group!'",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "MuteTime",
                table: "Groups",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "'03:00:00'",
                oldClrType: typeof(TimeSpan),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<short>(
                name: "MaxWarns",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValueSql: "3",
                oldClrType: typeof(short),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<long>(
                name: "GroupId",
                table: "ForceJoinChannels",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WelcomeMessage",
                table: "Groups",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldDefaultValueSql: "'Welcome To Group!'");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "MuteTime",
                table: "Groups",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "TEXT",
                oldDefaultValueSql: "'00:00:00'");

            migrationBuilder.AlterColumn<short>(
                name: "MaxWarns",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "INTEGER",
                oldDefaultValueSql: "3");

            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "ForceJoinChannels",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ForceJoinChannels",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForceJoinChannels",
                table: "ForceJoinChannels",
                column: "Id");
        }
    }
}
