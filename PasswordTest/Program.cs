using System;
using BCrypt.Net;

public class Program
{
    public static void Main()
    {
        string password = "password123";
        
        // Generar un nuevo hash
        string newHash = BCrypt.Net.BCrypt.HashPassword(password);
        Console.WriteLine($"Nuevo hash para 'password123': {newHash}");
        
        // Hash que esperamos esté en la base de datos (del ApplicationDbContext.cs)
        string expectedHash = "$2a$11$Xed1MsTJ.11zGnnvwDLGaeGav13ki.M4gEB5LSGg/vhxtWT5FC8Xm";
        
        // Verificar si la contraseña coincide con el hash esperado
        bool isValid = BCrypt.Net.BCrypt.Verify(password, expectedHash);
        Console.WriteLine($"¿La contraseña 'password123' coincide con el hash esperado? {isValid}");
        
        // Probar también con otras posibles contraseñas
        string[] testPasswords = { "password123", "admin123", "123456", "admin" };
        
        foreach (string testPassword in testPasswords)
        {
            bool valid = BCrypt.Net.BCrypt.Verify(testPassword, expectedHash);
            Console.WriteLine($"Contraseña '{testPassword}': {valid}");
        }
    }
}