using Microsoft.EntityFrameworkCore.Migrations;

namespace PersonalSafety.Migrations
{
    public partial class AddNewRolesToDbContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "08e4a355-fb13-49e7-8617-cc121f693c91");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "83db8973-ee63-4677-85c7-5f96f4df48d2");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "263dc839-5e82-4dbb-a06e-8ab047056a68", "8855d8e7-b43a-460a-b95a-6ba83e131181", "Admin", "ADMIN" },
                    { "9c36997b-e408-4bcd-a844-c2a1495379c7", "0666a7a7-09bb-4065-a53e-5aaa9d789ddd", "Personnel", "PERSONNEL" },
                    { "72630ad4-895a-4ad5-ba34-ce9d172c48af", "c9045169-1e3e-4c3f-857a-3abfdc4df7c7", "Agent", "AGENT" },
                    { "3b13be57-7cd4-4822-975f-139a2836e1ac", "75d465d3-85c8-4dfe-985c-555ca6f65a03", "Rescuer", "RESCUER" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "263dc839-5e82-4dbb-a06e-8ab047056a68");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3b13be57-7cd4-4822-975f-139a2836e1ac");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "72630ad4-895a-4ad5-ba34-ce9d172c48af");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9c36997b-e408-4bcd-a844-c2a1495379c7");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "08e4a355-fb13-49e7-8617-cc121f693c91", "bba279eb-ea38-4016-974f-f5ea6e12d05b", "Admin", "ADMIN" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "83db8973-ee63-4677-85c7-5f96f4df48d2", "a3fddb76-c6ef-47cc-8540-16148f8962e2", "Personnel", "PERSONNEL" });
        }
    }
}
