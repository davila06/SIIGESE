using Infrastructure.Data;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

// Configurar el DbContext
var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=SINSEGDb;Trusted_Connection=true;";
var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlServer(connectionString);

using var context = new ApplicationDbContext(optionsBuilder.Options);

// Verificar si ya hay datos
var existingCobros = await context.Cobros.CountAsync();
Console.WriteLine($"Cobros existentes: {existingCobros}");

if (existingCobros == 0)
{
    // Insertar datos de prueba
    var cobros = new List<Cobro>
    {
        new Cobro
        {
            NumeroRecibo = "REC-2025-001",
            PolizaId = 1,
            NumeroPoliza = "POL-2025-001",
            ClienteNombre = "Juan",
            ClienteApellido = "Pérez",
            FechaVencimiento = new DateTime(2025, 1, 15),
            MontoTotal = 250000.00m,
            Moneda = "CRC",
            Estado = EstadoCobro.Pendiente,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            IsDeleted = false
        },
        new Cobro
        {
            NumeroRecibo = "REC-2025-002",
            PolizaId = 2,
            NumeroPoliza = "POL-2025-002",
            ClienteNombre = "María",
            ClienteApellido = "González",
            FechaVencimiento = new DateTime(2025, 1, 20),
            MontoTotal = 180000.00m,
            Moneda = "CRC",
            Estado = EstadoCobro.Cobrado,
            FechaCobro = DateTime.UtcNow.AddDays(-5),
            MontoCobrado = 180000.00m,
            MetodoPago = MetodoPago.Transferencia,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            IsDeleted = false
        },
        new Cobro
        {
            NumeroRecibo = "REC-2025-003",
            PolizaId = 3,
            NumeroPoliza = "POL-2025-003",
            ClienteNombre = "Carlos",
            ClienteApellido = "Rodríguez",
            FechaVencimiento = new DateTime(2024, 12, 30),
            MontoTotal = 320000.00m,
            Moneda = "CRC",
            Estado = EstadoCobro.Vencido,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            IsDeleted = false
        },
        new Cobro
        {
            NumeroRecibo = "REC-2025-004",
            PolizaId = 4,
            NumeroPoliza = "POL-2025-004",
            ClienteNombre = "Ana",
            ClienteApellido = "Martínez",
            FechaVencimiento = new DateTime(2025, 2, 1),
            MontoTotal = 150.00m,
            Moneda = "USD",
            Estado = EstadoCobro.Pendiente,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            IsDeleted = false
        },
        new Cobro
        {
            NumeroRecibo = "REC-2025-005",
            PolizaId = 5,
            NumeroPoliza = "POL-2025-005",
            ClienteNombre = "Roberto",
            ClienteApellido = "López",
            FechaVencimiento = new DateTime(2025, 1, 25),
            MontoTotal = 280000.00m,
            Moneda = "CRC",
            Estado = EstadoCobro.Pendiente,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            IsDeleted = false
        }
    };

    context.Cobros.AddRange(cobros);
    await context.SaveChangesAsync();
    
    Console.WriteLine($"Se insertaron {cobros.Count} cobros de prueba.");
}
else
{
    Console.WriteLine("Ya existen cobros en la base de datos.");
}

Console.WriteLine("Datos de prueba procesados exitosamente.");