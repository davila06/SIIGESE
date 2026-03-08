using System;

Console.WriteLine("Generando hash BCrypt para 'admin123'...\n");

string password = "admin123";
string hash = BCrypt.Net.BCrypt.HashPassword(password, 11);

Console.WriteLine($"Password: {password}");
Console.WriteLine($"Hash BCrypt: {hash}");
Console.WriteLine("\n--- Script SQL ---");
Console.WriteLine($"UPDATE Users SET PasswordHash = '{hash}', IsActive = 1, IsDeleted = 0 WHERE Email = 'admin@sinseg.com';");
Console.WriteLine("\nEjecuta este UPDATE en tu base de datos Azure SQL.");
