using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailConfigTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SmtpServer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SmtpPort = table.Column<int>(type: "int", nullable: false),
                    FromEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FromName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UseSSL = table.Column<bool>(type: "bit", nullable: false),
                    UseTLS = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CompanyAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CompanyPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CompanyWebsite = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompanyLogo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TimeoutSeconds = table.Column<int>(type: "int", nullable: false),
                    MaxRetries = table.Column<int>(type: "int", nullable: false),
                    LastTested = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastTestSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    LastTestError = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailConfigs", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 17, 49, 8, 188, DateTimeKind.Utc).AddTicks(8264));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 17, 49, 8, 188, DateTimeKind.Utc).AddTicks(8272));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 17, 49, 8, 188, DateTimeKind.Utc).AddTicks(8273));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 10, 17, 49, 8, 390, DateTimeKind.Utc).AddTicks(9329));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "LastPasswordChangeAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 10, 17, 49, 8, 188, DateTimeKind.Utc).AddTicks(8456), new DateTime(2025, 10, 10, 17, 49, 8, 390, DateTimeKind.Utc).AddTicks(3333), "$2a$11$S3xQGzPo8/4EN8B/gY.AUOiKbE.b79bDyHPZrYibXp3yRTffQ1fLi" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailConfigs_ConfigName",
                table: "EmailConfigs",
                column: "ConfigName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailConfigs_IsActive",
                table: "EmailConfigs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EmailConfigs_IsDefault",
                table: "EmailConfigs",
                column: "IsDefault");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailConfigs");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 5, 4, 6, 43, 432, DateTimeKind.Utc).AddTicks(2376));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 5, 4, 6, 43, 432, DateTimeKind.Utc).AddTicks(2384));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 5, 4, 6, 43, 432, DateTimeKind.Utc).AddTicks(2385));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 5, 4, 6, 43, 607, DateTimeKind.Utc).AddTicks(6992));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "LastPasswordChangeAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 5, 4, 6, 43, 432, DateTimeKind.Utc).AddTicks(2543), new DateTime(2025, 10, 5, 4, 6, 43, 607, DateTimeKind.Utc).AddTicks(6266), "$2a$11$gTPJsyPi3DZ6F.bHZ62Qv.VTvNEiOiOy25IHXu1eITRS44x9s/mxm" });
        }
    }
}
