using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCobrosTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cobros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroRecibo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PolizaId = table.Column<int>(type: "int", nullable: false),
                    NumeroPoliza = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClienteNombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClienteApellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCobro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoCobrado = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    MetodoPago = table.Column<int>(type: "int", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UsuarioCobroId = table.Column<int>(type: "int", nullable: true),
                    UsuarioCobroNombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cobros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cobros_Polizas_PolizaId",
                        column: x => x.PolizaId,
                        principalTable: "Polizas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cobros_Users_UsuarioCobroId",
                        column: x => x.UsuarioCobroId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 5, 1, 19, 5, 80, DateTimeKind.Utc).AddTicks(2707));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 5, 1, 19, 5, 80, DateTimeKind.Utc).AddTicks(2714));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 5, 1, 19, 5, 80, DateTimeKind.Utc).AddTicks(2716));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 5, 1, 19, 5, 293, DateTimeKind.Utc).AddTicks(2823));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "LastPasswordChangeAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 5, 1, 19, 5, 80, DateTimeKind.Utc).AddTicks(2981), new DateTime(2025, 10, 5, 1, 19, 5, 293, DateTimeKind.Utc).AddTicks(2067), "$2a$11$oOFuS5G5NQwxjfQXMRfTd.N5oRI7cpiTiq9HfWBfrqT.LbRplzve2" });

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_Estado",
                table: "Cobros",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_FechaVencimiento",
                table: "Cobros",
                column: "FechaVencimiento");

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_NumeroRecibo",
                table: "Cobros",
                column: "NumeroRecibo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_PolizaId",
                table: "Cobros",
                column: "PolizaId");

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_UsuarioCobroId",
                table: "Cobros",
                column: "UsuarioCobroId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cobros");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 4, 23, 40, 21, 198, DateTimeKind.Utc).AddTicks(8468));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 4, 23, 40, 21, 198, DateTimeKind.Utc).AddTicks(8472));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 4, 23, 40, 21, 198, DateTimeKind.Utc).AddTicks(8520));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 4, 23, 40, 21, 416, DateTimeKind.Utc).AddTicks(1997));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "LastPasswordChangeAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 4, 23, 40, 21, 198, DateTimeKind.Utc).AddTicks(8629), new DateTime(2025, 10, 4, 23, 40, 21, 416, DateTimeKind.Utc).AddTicks(1338), "$2a$11$eAsTv1iZ0Cqj2oqn.Kiy/e5GJ/pgZ4SUs.i2bmWbOBEf6nqYMEvXq" });
        }
    }
}
