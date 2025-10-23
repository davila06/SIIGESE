using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

class LoginTest
{
    static async Task Main(string[] args)
    {
        var client = new HttpClient();
        var baseUrl = "http://localhost:5000";
        
        // Datos de login
        var loginData = new
        {
            email = "admin@sinseg.com",
            password = "password123"
        };
        
        var json = JsonConvert.SerializeObject(loginData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        try
        {
            Console.WriteLine($"Probando login con:");
            Console.WriteLine($"Email: {loginData.email}");
            Console.WriteLine($"Password: {loginData.password}");
            Console.WriteLine($"URL: {baseUrl}/api/auth/login");
            
            var response = await client.PostAsync($"{baseUrl}/api/auth/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"\nRespuesta del servidor:");
            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine($"Content: {responseContent}");
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("\n✅ LOGIN EXITOSO!");
                
                // Probar endpoint de cobros
                var responseJson = JsonConvert.DeserializeObject<dynamic>(responseContent);
                var token = responseJson.token;
                
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.ToString());
                
                Console.WriteLine("\nProbando endpoint de cobros...");
                var cobrosResponse = await client.GetAsync($"{baseUrl}/api/cobros");
                var cobrosContent = await cobrosResponse.Content.ReadAsStringAsync();
                
                Console.WriteLine($"Cobros Status: {cobrosResponse.StatusCode}");
                Console.WriteLine($"Cobros Content: {cobrosContent}");
            }
            else
            {
                Console.WriteLine("\n❌ LOGIN FALLÓ");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}