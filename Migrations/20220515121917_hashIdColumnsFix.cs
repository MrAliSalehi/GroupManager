using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupManager.Migrations
{
    public partial class hashIdColumnsFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimeBasedFunctionHashId",
                table: "Groups",
                newName: "TimeBasedUnmuteFuncHashId");

            migrationBuilder.AddColumn<string>(
                name: "TimeBasedMuteFuncHashId",
                table: "Groups",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeBasedMuteFuncHashId",
                table: "Groups");

            migrationBuilder.RenameColumn(
                name: "TimeBasedUnmuteFuncHashId",
                table: "Groups",
                newName: "TimeBasedFunctionHashId");
        }
    }
}
