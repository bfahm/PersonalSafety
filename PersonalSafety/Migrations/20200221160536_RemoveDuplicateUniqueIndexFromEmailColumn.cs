using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PersonalSafety.Migrations
{
    public partial class RemoveDuplicateUniqueIndexFromEmailColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59f054cd-022d-400f-ada8-550707b49311");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Birthday", "BloodType", "ConcurrencyStamp", "CurrentAddress", "CurrentInvolvement", "CurrentOngoingEvent", "Email", "EmailConfirmed", "FullName", "LockoutEnabled", "LockoutEnd", "MedicalHistoryNotes", "NationalId", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "0d6ba48a-066f-4964-b2e5-c3eb082089c3", 0, new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "2664a7b1-9f20-43af-9d47-7f417b925e99", "unknown...", 0, 0, "user@user.com", false, "Test User", false, null, "none...", "29700000000", null, null, null, "01010101010", false, "49d6f3fa-fd51-422a-817c-c6dd8e1b44db", false, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "0d6ba48a-066f-4964-b2e5-c3eb082089c3");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Birthday", "BloodType", "ConcurrencyStamp", "CurrentAddress", "CurrentInvolvement", "CurrentOngoingEvent", "Email", "EmailConfirmed", "FullName", "LockoutEnabled", "LockoutEnd", "MedicalHistoryNotes", "NationalId", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "59f054cd-022d-400f-ada8-550707b49311", 0, new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "40ef569c-7021-42e0-bad6-d79c5e68b81a", "unknown...", 0, 0, "user@user.com", false, "Test User", false, null, "none...", "29700000000", null, null, null, "01010101010", false, "0ba37087-33ae-4f59-a680-de8f14c520b0", false, null });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");
        }
    }
}
