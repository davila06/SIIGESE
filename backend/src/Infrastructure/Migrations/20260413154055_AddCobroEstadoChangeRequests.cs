using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCobroEstadoChangeRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CobroEstadoChangeRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CobroId = table.Column<int>(type: "int", nullable: false),
                    EstadoActual = table.Column<int>(type: "int", nullable: false),
                    EstadoSolicitado = table.Column<int>(type: "int", nullable: false),
                    EstadoSolicitud = table.Column<int>(type: "int", nullable: false),
                    MotivoSolicitud = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MotivoDecision = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SolicitadoPorUserId = table.Column<int>(type: "int", nullable: false),
                    SolicitadoPorNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SolicitadoPorEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ResueltoPorUserId = table.Column<int>(type: "int", nullable: true),
                    ResueltoPorNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ResueltoAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CobroEstadoChangeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CobroEstadoChangeRequests_Cobros_CobroId",
                        column: x => x.CobroId,
                        principalTable: "Cobros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 13, 15, 40, 53, 523, DateTimeKind.Utc).AddTicks(4477));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 13, 15, 40, 53, 523, DateTimeKind.Utc).AddTicks(4482));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 13, 15, 40, 53, 523, DateTimeKind.Utc).AddTicks(4484));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 13, 15, 40, 53, 729, DateTimeKind.Utc).AddTicks(9901));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 13, 15, 40, 53, 523, DateTimeKind.Utc).AddTicks(4609), "$2a$11$nvFBWUuOLKdSl7IFhZOHQeBu3OpvRzMVp5nq9mRHel11nRd45o4le" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_ExpiresAt",
                table: "PasswordResetTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_CobroEstadoChangeRequests_CobroId",
                table: "CobroEstadoChangeRequests",
                column: "CobroId");

            migrationBuilder.CreateIndex(
                name: "IX_CobroEstadoChangeRequests_CreatedAt",
                table: "CobroEstadoChangeRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CobroEstadoChangeRequests_EstadoSolicitud",
                table: "CobroEstadoChangeRequests",
                column: "EstadoSolicitud");

            migrationBuilder.CreateIndex(
                name: "IX_CobroEstadoChangeRequests_SolicitadoPorUserId",
                table: "CobroEstadoChangeRequests",
                column: "SolicitadoPorUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CobroEstadoChangeRequests");

            migrationBuilder.DropIndex(
                name: "IX_PasswordResetTokens_ExpiresAt",
                table: "PasswordResetTokens");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 11, 5, 52, 25, 544, DateTimeKind.Utc).AddTicks(4684));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 11, 5, 52, 25, 544, DateTimeKind.Utc).AddTicks(4692));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 11, 5, 52, 25, 544, DateTimeKind.Utc).AddTicks(4694));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 11, 5, 52, 25, 696, DateTimeKind.Utc).AddTicks(6682));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 11, 5, 52, 25, 544, DateTimeKind.Utc).AddTicks(4963), "$2a$11$mcl9ymhKGXTXQ1xw2lEFP.NCjBkI5Hg1FuRgfhtFHu0pKbT6Sb7ce" });
        }
    }
}
