using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PersonalSafety.Migrations
{
    public partial class AddCreationAndModificationDateSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2e24b5ef-c59f-407d-ba1b-574afd031716");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c61803b8-6bbd-42bd-9161-8f5620aaba90");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "SOSRequests",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "SOSRequests",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "Events",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Events",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "5c622cc9-672c-4e74-97a9-d4bcfb6e17af", "a8f8c800-1e45-4854-b539-a6e9667b1630", "Admin", "ADMIN" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "0ce5e16d-c778-4304-a41d-ca7a7880337c", "905e873b-478e-48da-86bd-c2d22383517a", "Personnel", "PERSONNEL" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0ce5e16d-c778-4304-a41d-ca7a7880337c");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5c622cc9-672c-4e74-97a9-d4bcfb6e17af");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "SOSRequests");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "SOSRequests");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Events");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "2e24b5ef-c59f-407d-ba1b-574afd031716", "5698c263-36d5-42da-98a0-e9a128661cfc", "Admin", "ADMIN" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "c61803b8-6bbd-42bd-9161-8f5620aaba90", "9adf8661-0033-43de-9767-bb8b75230884", "Personnel", "PERSONNEL" });
        }
    }
}
