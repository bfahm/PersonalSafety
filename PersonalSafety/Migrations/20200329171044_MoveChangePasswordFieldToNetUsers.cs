using Microsoft.EntityFrameworkCore.Migrations;

namespace PersonalSafety.Migrations
{
    public partial class MoveChangePasswordFieldToNetUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "59ad7863-b3b8-4770-88ac-daba55cc9493");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ef2fba56-130b-4ba9-b960-88d02bf5ce76");

            migrationBuilder.DropColumn(
                name: "IsFirstLogin",
                table: "Personnels");

            migrationBuilder.AddColumn<bool>(
                name: "ForceChangePassword",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "08e4a355-fb13-49e7-8617-cc121f693c91", "bba279eb-ea38-4016-974f-f5ea6e12d05b", "Admin", "ADMIN" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "83db8973-ee63-4677-85c7-5f96f4df48d2", "a3fddb76-c6ef-47cc-8540-16148f8962e2", "Personnel", "PERSONNEL" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "08e4a355-fb13-49e7-8617-cc121f693c91");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "83db8973-ee63-4677-85c7-5f96f4df48d2");

            migrationBuilder.DropColumn(
                name: "ForceChangePassword",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "IsFirstLogin",
                table: "Personnels",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "ef2fba56-130b-4ba9-b960-88d02bf5ce76", "bf987213-fd12-4810-8ddf-e77d4ff10653", "Admin", "ADMIN" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "59ad7863-b3b8-4770-88ac-daba55cc9493", "7d67d6ff-e1cc-4621-8d43-f250c0e99a66", "Personnel", "PERSONNEL" });
        }
    }
}
