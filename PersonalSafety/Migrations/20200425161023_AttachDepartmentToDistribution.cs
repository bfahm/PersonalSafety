using Microsoft.EntityFrameworkCore.Migrations;

namespace PersonalSafety.Migrations
{
    public partial class AttachDepartmentToDistribution : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Departments");

            migrationBuilder.AddColumn<int>(
                name: "DistributionId",
                table: "Departments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DistributionId",
                table: "Departments",
                column: "DistributionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Distributions_DistributionId",
                table: "Departments",
                column: "DistributionId",
                principalTable: "Distributions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Distributions_DistributionId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_DistributionId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "DistributionId",
                table: "Departments");

            migrationBuilder.AddColumn<int>(
                name: "City",
                table: "Departments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
