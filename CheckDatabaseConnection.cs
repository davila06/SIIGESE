using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Infrastructure.Data;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true);

var configuration = builder.Build();

// Obtener cadena de conexión igual que en Program.cs
var connectionString = configuration.GetConnectionString("DefaultConnection") 
    ?? configuration.GetConnectionString("LocalDbConnection");

Console.WriteLine("=== INFORMACIÓN DE CONEXIÓN A BASE DE DATOS ===");
Console.WriteLine();
Console.WriteLine($"Entorno actual: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}");
Console.WriteLine();

if (connectionString != null)
{
    // Ocultar la contraseña para mostrar información segura
    var safeConnectionString = connectionString;
    if (connectionString.Contains("Password="))
    {
        var parts = connectionString.Split(';');
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Trim().StartsWith("Password="))
            {
                parts[i] = "Password=***OCULTA***";
            }
        }
        safeConnectionString = string.Join(";", parts);
    }

    Console.WriteLine("Cadena de conexión (sin contraseña):");
    Console.WriteLine(safeConnectionString);
    Console.WriteLine();

    // Determinar tipo de base de datos
    if (connectionString.Contains("database.windows.net"))
    {
        Console.WriteLine("🔵 TIPO DE BD: Azure SQL Database (Nube)");
        
        // Extraer información específica
        if (connectionString.Contains("siinadseg-sql-5307.database.windows.net"))
        {
            Console.WriteLine("📡 Servidor: siinadseg-sql-5307.database.windows.net");
        }
        
        if (connectionString.Contains("Initial Catalog=SiinadsegDB"))
        {
            Console.WriteLine("🗃️  Base de Datos: SiinadsegDB");
        }
        
        if (connectionString.Contains("User ID=siinadsegadmin"))
        {
            Console.WriteLine("👤 Usuario: siinadsegadmin");
        }
    }
    else if (connectionString.Contains("(localdb)"))
    {
        Console.WriteLine("🟢 TIPO DE BD: SQL Server LocalDB (Local)");
        Console.WriteLine("🗃️  Base de Datos: SinsegAppDb");
        Console.WriteLine("🔐 Autenticación: Windows (Trusted_Connection)");
    }
    else
    {
        Console.WriteLine("❓ Tipo de BD: Desconocido");
    }
}
else
{
    Console.WriteLine("❌ No se encontró cadena de conexión");
}

Console.WriteLine();
Console.WriteLine("=== CONFIGURACIONES DISPONIBLES ===");
Console.WriteLine();

var defaultConnection = configuration.GetConnectionString("DefaultConnection");
var localConnection = configuration.GetConnectionString("LocalDbConnection");

if (defaultConnection != null)
{
    Console.WriteLine("✅ DefaultConnection: Configurada");
    if (defaultConnection.Contains("database.windows.net"))
    {
        Console.WriteLine("   → Azure SQL Database");
    }
}
else
{
    Console.WriteLine("❌ DefaultConnection: No configurada");
}

if (localConnection != null)
{
    Console.WriteLine("✅ LocalDbConnection: Configurada");
    Console.WriteLine("   → SQL Server LocalDB");
}
else
{
    Console.WriteLine("❌ LocalDbConnection: No configurada");
}

Console.WriteLine();
Console.WriteLine("Presiona cualquier tecla para salir...");
Console.ReadKey();