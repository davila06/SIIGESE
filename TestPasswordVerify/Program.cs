using System;

class Program
{
    static void Main()
    {
        string password = "password123";
        string hash = "$2a$11$S3xQGzPo8/4EN8B/gY.AUOiKbE.b79bDyHPZrYibXp3yRTffQ1fLi";
        
        Console.WriteLine($"Testing password: {password}");
        Console.WriteLine($"Against hash: {hash}");
        
        bool isValid = BCrypt.Net.BCrypt.Verify(password, hash);
        
        Console.WriteLine($"Result: {isValid}");
        
        if (!isValid)
        {
            Console.WriteLine("\nTrying to generate a new hash for the same password:");
            string newHash = BCrypt.Net.BCrypt.HashPassword(password);
            Console.WriteLine($"New hash: {newHash}");
            Console.WriteLine($"Verify with new hash: {BCrypt.Net.BCrypt.Verify(password, newHash)}");
        }
    }
}
