using Microsoft.EntityFrameworkCore.Migrations;

namespace PersonalSafety.Migrations
{
    public partial class AddDistribution : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Distributions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(nullable: false),
                    Value = table.Column<string>(nullable: true),
                    ParentId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Distributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Distributions_Distributions_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Distributions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Distributions_ParentId",
                table: "Distributions",
                column: "ParentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Distributions");
        }
    }
}
