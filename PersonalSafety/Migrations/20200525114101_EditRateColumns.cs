using Microsoft.EntityFrameworkCore.Migrations;

namespace PersonalSafety.Migrations
{
    public partial class EditRateColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rate",
                table: "Personnels");

            migrationBuilder.DropColumn(
                name: "Rate",
                table: "Clients");

            migrationBuilder.AddColumn<float>(
                name: "RateAverage",
                table: "Personnels",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "RateCount",
                table: "Personnels",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "RateAverage",
                table: "Clients",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "RateCount",
                table: "Clients",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RateAverage",
                table: "Personnels");

            migrationBuilder.DropColumn(
                name: "RateCount",
                table: "Personnels");

            migrationBuilder.DropColumn(
                name: "RateAverage",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "RateCount",
                table: "Clients");

            migrationBuilder.AddColumn<float>(
                name: "Rate",
                table: "Personnels",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Rate",
                table: "Clients",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
