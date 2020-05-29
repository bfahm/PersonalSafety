using Microsoft.EntityFrameworkCore.Migrations;

namespace PersonalSafety.Migrations
{
    public partial class AssignClientAndEventToCity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Events_InvolvedInEventId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Events_PublicEventId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_InvolvedInEventId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_PublicEventId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "InvolvedInEventId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PublicEventId",
                table: "Clients");

            migrationBuilder.AddColumn<int>(
                name: "NearestCityId",
                table: "Events",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "CenterLatitude",
                table: "Distributions",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CenterLongitude",
                table: "Distributions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastKnownCityId",
                table: "Clients",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_NearestCityId",
                table: "Events",
                column: "NearestCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_LastKnownCityId",
                table: "Clients",
                column: "LastKnownCityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Distributions_LastKnownCityId",
                table: "Clients",
                column: "LastKnownCityId",
                principalTable: "Distributions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Distributions_NearestCityId",
                table: "Events",
                column: "NearestCityId",
                principalTable: "Distributions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Distributions_LastKnownCityId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Distributions_NearestCityId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_NearestCityId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Clients_LastKnownCityId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "NearestCityId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CenterLatitude",
                table: "Distributions");

            migrationBuilder.DropColumn(
                name: "CenterLongitude",
                table: "Distributions");

            migrationBuilder.DropColumn(
                name: "LastKnownCityId",
                table: "Clients");

            migrationBuilder.AddColumn<int>(
                name: "InvolvedInEventId",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublicEventId",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_InvolvedInEventId",
                table: "Clients",
                column: "InvolvedInEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_PublicEventId",
                table: "Clients",
                column: "PublicEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Events_InvolvedInEventId",
                table: "Clients",
                column: "InvolvedInEventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Events_PublicEventId",
                table: "Clients",
                column: "PublicEventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
