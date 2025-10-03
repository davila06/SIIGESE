using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseFileTypeLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 22, 47, 0, 603, DateTimeKind.Utc).AddTicks(4206));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 22, 47, 0, 603, DateTimeKind.Utc).AddTicks(4217));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 22, 47, 0, 603, DateTimeKind.Utc).AddTicks(4218));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 22, 47, 0, 822, DateTimeKind.Utc).AddTicks(427));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 1, 22, 47, 0, 603, DateTimeKind.Utc).AddTicks(4435), "$2a$11$8nhPSmjjsP15ANy.C5nkUua8yAmLgmkY.Xpq/NYxkJ2PpSnJ0Gcju" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 17, 5, 0, 264, DateTimeKind.Utc).AddTicks(7399));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 17, 5, 0, 264, DateTimeKind.Utc).AddTicks(7403));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 17, 5, 0, 264, DateTimeKind.Utc).AddTicks(7404));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 1, 17, 5, 0, 407, DateTimeKind.Utc).AddTicks(3752));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 1, 17, 5, 0, 264, DateTimeKind.Utc).AddTicks(7534), "$2a$11$Xed1MsTJ.11zGnnvwDLGaeGav13ki.M4gEB5LSGg/vhxtWT5FC8Xm" });
        }
    }
}
