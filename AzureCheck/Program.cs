using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        string connectionString = "Server=siinadseg-sql-5307.database.windows.net;Database=SiinadsegDB;User Id=siinadsegadmin;Password=Siinadseg2024!SecurePass;Encrypt=True;TrustServerCertificate=True;";
        
        using (var connection = new SqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();
                Console.WriteLine("✓ Conexión exitosa a Azure SQL Database");
                
                // Verificar si el usuario admin existe
                var checkUserQuery = "SELECT Id, Email, UserName, PasswordHash FROM Users WHERE Email = @email";
                using (var command = new SqlCommand(checkUserQuery, connection))
                {
                    command.Parameters.AddWithValue("@email", "admin@sinseg.com");
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            Console.WriteLine($"✓ Usuario encontrado:");
                            Console.WriteLine($"  ID: {reader["Id"]}");
                            Console.WriteLine($"  Email: {reader["Email"]}");
                            Console.WriteLine($"  UserName: {reader["UserName"]}");
                            Console.WriteLine($"  PasswordHash: {reader["PasswordHash"]}");
                        }
                        else
                        {
                            Console.WriteLine("✗ Usuario admin@sinseg.com NO encontrado");
                        }
                    }
                }
                
                Console.WriteLine("\n--- Verificando tabla Users ---");
                var countQuery = "SELECT COUNT(*) FROM Users";
                using (var command = new SqlCommand(countQuery, connection))
                {
                    var count = await command.ExecuteScalarAsync();
                    Console.WriteLine($"Total de usuarios en la tabla: {count}");
                }
                
                // Crear usuario admin si no existe
                var createUserQuery = @"
                    IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = @email)
                    BEGIN
                        INSERT INTO Users (Id, Email, UserName, PasswordHash, FirstName, LastName, IsActive, CreatedAt)
                        VALUES (@id, @email, @username, @password, @firstName, @lastName, 1, GETDATE())
                        PRINT 'Usuario admin creado exitosamente'
                    END
                    ELSE
                    BEGIN
                        PRINT 'Usuario admin ya existe'
                    END
                ";
                
                // Hash de la contraseña password123
                string password = "password123";
                string hashedPassword = HashPassword(password);
                
                using (var command = new SqlCommand(createUserQuery, connection))
                {
                    command.Parameters.AddWithValue("@id", "12345678-1234-1234-1234-123456789012");
                    command.Parameters.AddWithValue("@email", "admin@sinseg.com");
                    command.Parameters.AddWithValue("@username", "admin");
                    command.Parameters.AddWithValue("@password", hashedPassword);
                    command.Parameters.AddWithValue("@firstName", "Admin");
                    command.Parameters.AddWithValue("@lastName", "SINSEG");
                    
                    await command.ExecuteNonQueryAsync();
                    Console.WriteLine("✓ Comando de creación de usuario ejecutado");
                }
                
                // Verificar nuevamente
                using (var command = new SqlCommand(checkUserQuery, connection))
                {
                    command.Parameters.AddWithValue("@email", "admin@sinseg.com");
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            Console.WriteLine($"\n✓ Usuario confirmado:");
                            Console.WriteLine($"  ID: {reader["Id"]}");
                            Console.WriteLine($"  Email: {reader["Email"]}");
                            Console.WriteLine($"  UserName: {reader["UserName"]}");
                            Console.WriteLine($"  PasswordHash: {reader["PasswordHash"]}");
                            Console.WriteLine($"\nCredenciales para login:");
                            Console.WriteLine($"  Email: admin@sinseg.com");
                            Console.WriteLine($"  Password: password123");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
    
    static string HashPassword(string password)
    {
        // Simulación del hash usado por BCrypt
        using (var sha256 = SHA256.Create())
        {
            var saltedPassword = password + "simpleSalt";
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}