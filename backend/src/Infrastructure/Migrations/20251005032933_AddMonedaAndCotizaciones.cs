using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMonedaAndCotizaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Moneda",
                table: "Cobros",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Cotizaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroCotizacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NombreSolicitante = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoSeguro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Aseguradora = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MontoAsegurado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrimaCotizada = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FechaCotizacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Placa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Marca = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Modelo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Año = table.Column<int>(type: "int", nullable: true),
                    Cilindraje = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Genero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ocupacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DireccionInmueble = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TipoInmueble = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ValorInmueble = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cotizaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_Users_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_Email",
                table: "Cotizaciones",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_Estado",
                table: "Cotizaciones",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_FechaCotizacion",
                table: "Cotizaciones",
                column: "FechaCotizacion");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_NumeroCotizacion",
                table: "Cotizaciones",
                column: "NumeroCotizacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_TipoSeguro",
                table: "Cotizaciones",
                column: "TipoSeguro");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_UsuarioId",
                table: "Cotizaciones",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "Moneda",
                table: "Cobros");

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
        }
    }
}
