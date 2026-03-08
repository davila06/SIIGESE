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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}