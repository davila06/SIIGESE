using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReclamosModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reclamos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroReclamo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PolizaId = table.Column<int>(type: "int", nullable: false),
                    NumeroPoliza = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClienteNombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClienteApellido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaReclamo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaResolucion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TipoReclamo = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MontoReclamado = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MontoAprobado = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Moneda = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prioridad = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentosAdjuntos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsuarioAsignadoId = table.Column<int>(type: "int", nullable: true),
                    UsuarioAsignadoNombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaLimiteRespuesta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reclamos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reclamos_Polizas_PolizaId",
                        column: x => x.PolizaId,
                        principalTable: "Polizas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reclamos_Users_UsuarioAsignadoId",
                        column: x => x.UsuarioAsignadoId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Reclamos_PolizaId",
                table: "Reclamos",
                column: "PolizaId");

            migrationBuilder.CreateIndex(
                name: "IX_Reclamos_UsuarioAsignadoId",
                table: "Reclamos",
                column: "UsuarioAsignadoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reclamos");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 5, 3, 29, 31, 653, DateTimeKind.Utc).AddTicks(6921));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 5, 3, 29, 31, 653, DateTimeKind.Utc).AddTicks(6930));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 5, 3, 29, 31, 653, DateTimeKind.Utc).AddTicks(6932));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 5, 3, 29, 31, 957, DateTimeKind.Utc).AddTicks(1018));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "LastPasswordChangeAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 5, 3, 29, 31, 653, DateTimeKind.Utc).AddTicks(7296), new DateTime(2025, 10, 5, 3, 29, 31, 956, DateTimeKind.Utc).AddTicks(9868), "$2a$11$CmLRP9Jl1Vwz6hJmE2lYLejDJmrzwtD.fSv4TWOt16Vyl.ci9GBGS" });
        }
    }
}
