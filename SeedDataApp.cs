using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Check if we already have data
    var existingCobros = await context.Set<Domain.Entities.Cobro>().CountAsync();
    
    if (existingCobros == 0)
    {
        Console.WriteLine("Insertando datos de prueba en la tabla Cobros...");
        
        // Execute raw SQL to insert test data
        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO Cobros (
                NumeroPoliza,
                NumeroRecibo,
                ClienteNombre,
                ClienteApellido,
                MontoTotal,
                FechaVencimiento,
                Estado,
                MetodoPago,
                Moneda,
                PolizaId,
                CreatedAt,
                CreatedBy,
                IsDeleted
            ) VALUES 
            ('POL-2024-001', 'REC-2024-001', 'María', 'González', 125000.00, '2024-11-15', 0, 'Transferencia', 'CRC', 1, GETUTCDATE(), 1, 0),
            ('POL-2024-002', 'REC-2024-002', 'Carlos', 'Rodríguez', 180000.00, '2025-01-15', 0, 'Tarjeta', 'CRC', 2, GETUTCDATE(), 1, 0),
            ('POL-2024-003', 'REC-2024-003', 'Ana', 'Jiménez', 450.00, '2024-12-01', 1, 'Efectivo', 'USD', 3, GETUTCDATE(), 1, 0),
            ('POL-2024-004', 'REC-2024-004', 'Luis', 'Vargas', 320.50, '2025-02-10', 0, 'Cheque', 'EUR', 4, GETUTCDATE(), 1, 0),
            ('POL-2024-005', 'REC-2024-005', 'Patricia', 'Morales', 95000.00, '2024-10-30', 2, 'Transferencia', 'CRC', 5, GETUTCDATE(), 1, 0);
        ");
        
        // Update paid cobros
        await context.Database.ExecuteSqlRawAsync(@"
            UPDATE Cobros 
            SET MontoCobrado = MontoTotal, 
                FechaCobro = '2024-12-01',
                UsuarioCobroId = 1,
                UsuarioCobroNombre = 'admin'
            WHERE Estado = 1;
        ");
        
        Console.WriteLine("Datos de prueba insertados exitosamente!");
        
        // Verify data
        var count = await context.Set<Domain.Entities.Cobro>().CountAsync();
        Console.WriteLine($"Total de cobros en la base de datos: {count}");
    }
    else
    {
        Console.WriteLine($"Ya existen {existingCobros} cobros en la base de datos. No se insertarán datos duplicados.");
    }
}

Console.WriteLine("Proceso completado. Presiona cualquier tecla para continuar...");
Console.ReadKey();