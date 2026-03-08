// Script para generar hash BCrypt para "admin123"
#r "nuget: BCrypt.Net-Next, 4.0.3"

using BCrypt.Net;

var password = "admin123";
var hash = BCrypt.HashPassword(password, 11);

Console.WriteLine("Password: " + password);
Console.WriteLine("Hash BCrypt: " + hash);
Console.WriteLine("\nScript SQL:");
Console.WriteLine("UPDATE Users SET PasswordHash = '" + hash + "' WHERE Email = 'admin@sinseg.com';");
