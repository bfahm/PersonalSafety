using Microsoft.EntityFrameworkCore.Migrations;

namespace PersonalSafety.Migrations
{
    public partial class FixForeignKeyForNewModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Clients_AspNetUsers_ClientId",
                table: "Clients",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Personnels_AspNetUsers_PersonnelId",
                table: "Personnels",
                column: "PersonnelId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_AspNetUsers_ClientId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Personnels_AspNetUsers_PersonnelId",
                table: "Personnels");
        }
    }
}
