using Microsoft.EntityFrameworkCore.Migrations;

namespace PersonalSafety.Migrations
{
    public partial class ModifyTrackingKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientTrackings_AspNetUsers_ClientId",
                table: "ClientTrackings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClientTrackings",
                table: "ClientTrackings");

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                table: "ClientTrackings",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ClientTrackings",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClientTrackings",
                table: "ClientTrackings",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ClientTrackings_ClientId",
                table: "ClientTrackings",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientTrackings_AspNetUsers_ClientId",
                table: "ClientTrackings",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientTrackings_AspNetUsers_ClientId",
                table: "ClientTrackings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClientTrackings",
                table: "ClientTrackings");

            migrationBuilder.DropIndex(
                name: "IX_ClientTrackings_ClientId",
                table: "ClientTrackings");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ClientTrackings");

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                table: "ClientTrackings",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClientTrackings",
                table: "ClientTrackings",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientTrackings_AspNetUsers_ClientId",
                table: "ClientTrackings",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
