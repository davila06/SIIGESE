using System;
using BCrypt.Net;

namespace GenerateHash
{
    class Program
    {
        static void Main(string[] args)
        {
            string password = "admin123";
            string hash = BCrypt.HashPassword(password, 11);
            
            Console.WriteLine($"Password: {password}");
            Console.WriteLine($"Hash: {hash}");
            Console.WriteLine();
            Console.WriteLine("SQL Script:");
            Console.WriteLine($"UPDATE Users SET PasswordHash = '{hash}' WHERE Email = 'admin@sinseg.com';");
        }
    }
}
