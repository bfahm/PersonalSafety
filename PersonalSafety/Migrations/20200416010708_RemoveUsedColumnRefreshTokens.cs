using Microsoft.EntityFrameworkCore.Migrations;

namespace PersonalSafety.Migrations
{
    public partial class RemoveUsedColumnRefreshTokens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Used",
                table: "RefreshTokens");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Used",
                table: "RefreshTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
