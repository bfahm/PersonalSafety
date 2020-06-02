using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PersonalSafety.Migrations
{
    public partial class AddClientTrackingTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCoronaSusceptible",
                table: "Clients",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCoronaVictim",
                table: "Clients",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ClientTrackings",
                columns: table => new
                {
                    ClientId = table.Column<string>(nullable: false),
                    Latitude = table.Column<double>(nullable: false),
                    Longitude = table.Column<double>(nullable: false),
                    Time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientTrackings", x => x.ClientId);
                    table.ForeignKey(
                        name: "FK_ClientTrackings_AspNetUsers_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientTrackings");

            migrationBuilder.DropColumn(
                name: "IsCoronaSusceptible",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "IsCoronaVictim",
                table: "Clients");
        }
    }
}
