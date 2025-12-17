using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCorreoElectronicoToCobro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cobros_Polizas_PolizaId",
                table: "Cobros");

            migrationBuilder.DropForeignKey(
                name: "FK_Cobros_Users_UsuarioCobroId",
                table: "Cobros");

            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_Users_UsuarioId",
                table: "Cotizaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Reclamos_Polizas_PolizaId",
                table: "Reclamos");

            migrationBuilder.DropIndex(
                name: "IX_Polizas_NumeroPoliza",
                table: "Polizas");

            migrationBuilder.DropIndex(
                name: "IX_PasswordResetTokens_ExpiresAt",
                table: "PasswordResetTokens");

            migrationBuilder.DropIndex(
                name: "IX_PasswordResetTokens_Token",
                table: "PasswordResetTokens");

            migrationBuilder.DropIndex(
                name: "IX_EmailConfigs_ConfigName",
                table: "EmailConfigs");

            migrationBuilder.DropIndex(
                name: "IX_EmailConfigs_IsActive",
                table: "EmailConfigs");

            migrationBuilder.DropIndex(
                name: "IX_EmailConfigs_IsDefault",
                table: "EmailConfigs");

            migrationBuilder.DropIndex(
                name: "IX_Cotizaciones_Email",
                table: "Cotizaciones");

            migrationBuilder.DropIndex(
                name: "IX_Cotizaciones_Estado",
                table: "Cotizaciones");

            migrationBuilder.DropIndex(
                name: "IX_Cotizaciones_FechaCotizacion",
                table: "Cotizaciones");

            migrationBuilder.DropIndex(
                name: "IX_Cotizaciones_NumeroCotizacion",
                table: "Cotizaciones");

            migrationBuilder.DropIndex(
                name: "IX_Cotizaciones_TipoSeguro",
                table: "Cotizaciones");

            migrationBuilder.DropIndex(
                name: "IX_Cobros_Estado",
                table: "Cobros");

            migrationBuilder.DropIndex(
                name: "IX_Cobros_FechaVencimiento",
                table: "Cobros");

            migrationBuilder.DropIndex(
                name: "IX_Cobros_NumeroRecibo",
                table: "Cobros");

            migrationBuilder.DropIndex(
                name: "IX_Cobros_PolizaId",
                table: "Cobros");

            migrationBuilder.DropIndex(
                name: "IX_Cobros_UsuarioCobroId",
                table: "Cobros");

            migrationBuilder.DropColumn(
                name: "UsuarioAsignadoNombre",
                table: "Reclamos");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "PasswordResetTokens");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "PasswordResetTokens");

            migrationBuilder.DropColumn(
                name: "Cilindraje",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "DireccionInmueble",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "FechaVencimiento",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "Genero",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "Ocupacion",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "PrimaCotizada",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "TipoInmueble",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "ValorInmueble",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "ClienteApellido",
                table: "Cobros");

            migrationBuilder.DropColumn(
                name: "ClienteNombre",
                table: "Cobros");

            migrationBuilder.RenameColumn(
                name: "ClienteNombre",
                table: "Reclamos",
                newName: "NombreAsegurado");

            migrationBuilder.RenameColumn(
                name: "ClienteApellido",
                table: "Reclamos",
                newName: "ClienteNombreCompleto");

            migrationBuilder.RenameColumn(
                name: "MontoAsegurado",
                table: "Cotizaciones",
                newName: "Prima");

            migrationBuilder.AlterColumn<int>(
                name: "PolizaId",
                table: "Reclamos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "Reclamos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoReclamado",
                table: "Reclamos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DocumentosAdjuntos",
                table: "Reclamos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Placa",
                table: "Polizas",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroPoliza",
                table: "Polizas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "NombreAsegurado",
                table: "Polizas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Moneda",
                table: "Polizas",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "Modelo",
                table: "Polizas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Modalidad",
                table: "Polizas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Marca",
                table: "Polizas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Frecuencia",
                table: "Polizas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Aseguradora",
                table: "Polizas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Año",
                table: "Polizas",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Correo",
                table: "Polizas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroCedula",
                table: "Polizas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroTelefono",
                table: "Polizas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "PasswordResetTokens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "EmailConfigs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "SmtpServer",
                table: "EmailConfigs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "EmailConfigs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastTested",
                table: "EmailConfigs",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "LastTestError",
                table: "EmailConfigs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FromName",
                table: "EmailConfigs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "FromEmail",
                table: "EmailConfigs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "EmailConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConfigName",
                table: "EmailConfigs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CompanyWebsite",
                table: "EmailConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CompanyPhone",
                table: "EmailConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CompanyName",
                table: "EmailConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CompanyLogo",
                table: "EmailConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CompanyAddress",
                table: "EmailConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableSsl",
                table: "EmailConfigs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "TipoSeguro",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Placa",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroCotizacion",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "NombreSolicitante",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Moneda",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Modelo",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Marca",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Año",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Aseguradora",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Correo",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVigencia",
                table: "Cotizaciones",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Frecuencia",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Modalidad",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NombreAsegurado",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumeroCedula",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumeroPoliza",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumeroTelefono",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PerfilId",
                table: "Cotizaciones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "UsuarioCobroNombre",
                table: "Cobros",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioCobroId",
                table: "Cobros",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "Cobros",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroRecibo",
                table: "Cobros",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroPoliza",
                table: "Cobros",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoCobrado",
                table: "Cobros",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MetodoPago",
                table: "Cobros",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCobro",
                table: "Cobros",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteNombreCompleto",
                table: "Cobros",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CorreoElectronico",
                table: "Cobros",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 17, 6, 0, 40, 696, DateTimeKind.Utc).AddTicks(2574));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 17, 6, 0, 40, 696, DateTimeKind.Utc).AddTicks(2586));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 17, 6, 0, 40, 696, DateTimeKind.Utc).AddTicks(2588));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 17, 6, 0, 40, 848, DateTimeKind.Utc).AddTicks(175));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "LastPasswordChangeAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 17, 6, 0, 40, 696, DateTimeKind.Utc).AddTicks(3578), null, "$2a$11$CJLovmV.NTNLOh0qmFEPVuupea22NPMOGy4JHuojT5Op.UizUQwi." });

            migrationBuilder.CreateIndex(
                name: "IX_Polizas_NumeroPoliza",
                table: "Polizas",
                column: "NumeroPoliza");

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizaciones_Users_UsuarioId",
                table: "Cotizaciones",
                column: "UsuarioId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reclamos_Polizas_PolizaId",
                table: "Reclamos",
                column: "PolizaId",
                principalTable: "Polizas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_Users_UsuarioId",
                table: "Cotizaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Reclamos_Polizas_PolizaId",
                table: "Reclamos");

            migrationBuilder.DropIndex(
                name: "IX_Polizas_NumeroPoliza",
                table: "Polizas");

            migrationBuilder.DropColumn(
                name: "Año",
                table: "Polizas");

            migrationBuilder.DropColumn(
                name: "Correo",
                table: "Polizas");

            migrationBuilder.DropColumn(
                name: "NumeroCedula",
                table: "Polizas");

            migrationBuilder.DropColumn(
                name: "NumeroTelefono",
                table: "Polizas");

            migrationBuilder.DropColumn(
                name: "EnableSsl",
                table: "EmailConfigs");

            migrationBuilder.DropColumn(
                name: "Correo",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "FechaVigencia",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "Frecuencia",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "Modalidad",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "NombreAsegurado",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "NumeroCedula",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "NumeroPoliza",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "NumeroTelefono",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "PerfilId",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "ClienteNombreCompleto",
                table: "Cobros");

            migrationBuilder.DropColumn(
                name: "CorreoElectronico",
                table: "Cobros");

            migrationBuilder.RenameColumn(
                name: "NombreAsegurado",
                table: "Reclamos",
                newName: "ClienteNombre");

            migrationBuilder.RenameColumn(
                name: "ClienteNombreCompleto",
                table: "Reclamos",
                newName: "ClienteApellido");

            migrationBuilder.RenameColumn(
                name: "Prima",
                table: "Cotizaciones",
                newName: "MontoAsegurado");

            migrationBuilder.AlterColumn<int>(
                name: "PolizaId",
                table: "Reclamos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "Reclamos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoReclamado",
                table: "Reclamos",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentosAdjuntos",
                table: "Reclamos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "UsuarioAsignadoNombre",
                table: "Reclamos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Placa",
                table: "Polizas",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroPoliza",
                table: "Polizas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NombreAsegurado",
                table: "Polizas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Moneda",
                table: "Polizas",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Modelo",
                table: "Polizas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Modalidad",
                table: "Polizas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Marca",
                table: "Polizas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Frecuencia",
                table: "Polizas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Aseguradora",
                table: "Polizas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "PasswordResetTokens",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "PasswordResetTokens",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "PasswordResetTokens",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "EmailConfigs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SmtpServer",
                table: "EmailConfigs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "EmailConfigs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastTested",
                table: "EmailConfigs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastTestError",
                table: "EmailConfigs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FromName",
                table: "EmailConfigs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FromEmail",
                table: "EmailConfigs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "EmailConfigs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ConfigName",
                table: "EmailConfigs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyWebsite",
                table: "EmailConfigs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyPhone",
                table: "EmailConfigs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyName",
                table: "EmailConfigs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyLogo",
                table: "EmailConfigs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyAddress",
                table: "EmailConfigs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TipoSeguro",
                table: "Cotizaciones",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Placa",
                table: "Cotizaciones",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroCotizacion",
                table: "Cotizaciones",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NombreSolicitante",
                table: "Cotizaciones",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Moneda",
                table: "Cotizaciones",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Modelo",
                table: "Cotizaciones",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Marca",
                table: "Cotizaciones",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Cotizaciones",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Cotizaciones",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Año",
                table: "Cotizaciones",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Aseguradora",
                table: "Cotizaciones",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Cilindraje",
                table: "Cotizaciones",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DireccionInmueble",
                table: "Cotizaciones",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNacimiento",
                table: "Cotizaciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimiento",
                table: "Cotizaciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Genero",
                table: "Cotizaciones",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "Cotizaciones",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ocupacion",
                table: "Cotizaciones",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrimaCotizada",
                table: "Cotizaciones",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Cotizaciones",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoInmueble",
                table: "Cotizaciones",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorInmueble",
                table: "Cotizaciones",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UsuarioCobroNombre",
                table: "Cobros",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioCobroId",
                table: "Cobros",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "Cobros",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroRecibo",
                table: "Cobros",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroPoliza",
                table: "Cobros",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoCobrado",
                table: "Cobros",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "MetodoPago",
                table: "Cobros",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCobro",
                table: "Cobros",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "ClienteApellido",
                table: "Cobros",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClienteNombre",
                table: "Cobros",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

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
                name: "IX_Polizas_NumeroPoliza",
                table: "Polizas",
                column: "NumeroPoliza",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_ExpiresAt",
                table: "PasswordResetTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Token",
                table: "PasswordResetTokens",
                column: "Token",
                unique: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Cobros_Polizas_PolizaId",
                table: "Cobros",
                column: "PolizaId",
                principalTable: "Polizas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cobros_Users_UsuarioCobroId",
                table: "Cobros",
                column: "UsuarioCobroId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizaciones_Users_UsuarioId",
                table: "Cotizaciones",
                column: "UsuarioId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reclamos_Polizas_PolizaId",
                table: "Reclamos",
                column: "PolizaId",
                principalTable: "Polizas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
