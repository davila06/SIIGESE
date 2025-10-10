using System;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Domain.Entities;
using BCrypt.Net;

var connectionString = "Server=tcp:siinadseg-sql-83020.database.windows.net,1433;Initial Catalog=SiinadsegMaster;Persist Security Info=False;User ID=siinadsegadmin;Password=Siinadseg2024!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlServer(connectionString);

using var context = new ApplicationDbContext(optionsBuilder.Options);

// Verificar usuarios existentes
var users = await context.Users.Where(u => !u.IsDeleted).ToListAsync();
Console.WriteLine($"Usuarios encontrados: {users.Count}");
foreach (var u in users)
{
    Console.WriteLine($"- {u.Email} ({u.UserName})");
}

// Verificar si existe superadmin@miapp.com
var superAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == "superadmin@miapp.com");
if (superAdmin == null)
{
    Console.WriteLine("\nCreando usuario superadmin@miapp.com...");
    var hashedPassword = BCrypt.Net.BCrypt.HashPassword("SuperAdmin123!");
    var newUser = new User
    {
        UserName = "superadmin",
        Email = "superadmin@miapp.com",
        FirstName = "Super",
        LastName = "Admin",
        PasswordHash = hashedPassword,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "System",
        IsDeleted = false,
        RequiresPasswordChange = false,
        LastPasswordChangeAt = DateTime.UtcNow
    };
    context.Users.Add(newUser);
    await context.SaveChangesAsync();
    Console.WriteLine("Usuario creado exitosamente!");
    
    // Asignar rol Admin
    var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
    if (adminRole != null)
    {
        var userRole = new UserRole
        {
            UserId = newUser.Id,
            RoleId = adminRole.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            IsDeleted = false
        };
        context.UserRoles.Add(userRole);
        await context.SaveChangesAsync();
        Console.WriteLine("Rol Admin asignado!");
    }
}
else
{
    Console.WriteLine($"\nUsuario superadmin@miapp.com ya existe (ID: {superAdmin.Id})");
}

Console.WriteLine("\nListo!");
