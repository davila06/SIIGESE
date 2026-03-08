using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using System.Globalization;

namespace SiinadsegApi
{
    public class PolizasSearchFunction
    {
        private readonly ILogger<PolizasSearchFunction> _logger;

        public PolizasSearchFunction(ILogger<PolizasSearchFunction> logger)
        {
            _logger = logger;
        }

        // GET /api/polizas/search?q={searchTerm} - Búsqueda optimizada de pólizas
        [Function("polizas-search")]
        public async Task<IActionResult> SearchPolizas([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "polizas/search")] HttpRequest req)
        {
            _logger.LogInformation("Búsqueda de pólizas iniciada");

            try
            {
                string searchTerm = req.Query["q"].ToString();
                
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new BadRequestObjectResult(new { 
                        success = false, 
                        message = "Término de búsqueda requerido" 
                    });
                }

                // Simular datos de pólizas para demostración
                var allPolizas = GetSamplePolizas();
                
                // Aplicar búsqueda mejorada
                var filteredPolizas = SearchPolizasWithAdvancedLogic(allPolizas, searchTerm);

                var response = new
                {
                    success = true,
                    data = filteredPolizas,
                    message = $"Se encontraron {filteredPolizas.Count} pólizas",
                    searchTerm = searchTerm,
                    total = filteredPolizas.Count
                };

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de pólizas");
                return new StatusCodeResult(500);
            }
        }

        // GET /api/polizas - Obtener todas las pólizas
        [Function("polizas-getall")]
        public async Task<IActionResult> GetAllPolizas([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "polizas")] HttpRequest req)
        {
            _logger.LogInformation("Obteniendo todas las pólizas");

            try
            {
                var polizas = GetSamplePolizas();

                var response = new
                {
                    success = true,
                    data = polizas,
                    message = "Pólizas obtenidas exitosamente",
                    total = polizas.Count
                };

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo pólizas");
                return new StatusCodeResult(500);
            }
        }

        private List<object> SearchPolizasWithAdvancedLogic(List<object> polizas, string searchTerm)
        {
            var normalizedSearch = NormalizeText(searchTerm.ToLower().Trim());
            
            return polizas.Where(p => {
                // Convertir a dynamic para acceder a propiedades
                var poliza = JsonSerializer.Deserialize<dynamic>(JsonSerializer.Serialize(p));
                
                // Extraer campos para búsqueda
                string numeroPoliza = poliza.GetProperty("numeroPoliza").GetString() ?? "";
                string nombreAsegurado = poliza.GetProperty("nombreAsegurado").GetString() ?? "";
                string placa = poliza.GetProperty("placa").GetString() ?? "";
                
                // Búsqueda en número de póliza
                bool numeroMatch = NormalizeText(numeroPoliza.ToLower()).Contains(normalizedSearch);
                
                // Búsqueda avanzada en nombre
                bool nombreMatch = SearchInName(nombreAsegurado, normalizedSearch);
                
                // Búsqueda en placa
                bool placaMatch = NormalizeText(placa.ToLower()).Contains(normalizedSearch);
                
                return numeroMatch || nombreMatch || placaMatch;
            }).ToList();
        }

        private bool SearchInName(string nombre, string searchTerm)
        {
            var normalizedName = NormalizeText(nombre.ToLower());
            var normalizedSearch = searchTerm;
            
            // Búsqueda directa
            if (normalizedName.Contains(normalizedSearch))
            {
                return true;
            }
            
            // Búsqueda por palabras separadas
            var searchWords = normalizedSearch.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var nameWords = normalizedName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            // Verificar que todas las palabras de búsqueda estén en el nombre
            return searchWords.All(searchWord => 
                nameWords.Any(nameWord => 
                    nameWord.Contains(searchWord) || searchWord.Contains(nameWord)
                )
            );
        }

        private string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";
                
            // Remover acentos
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace("[^\\w\\s]", "") // Remover caracteres especiales
                .Trim();
        }

        private List<object> GetSamplePolizas()
        {
            return new List<object>
            {
                new
                {
                    id = 1,
                    numeroPoliza = "POL-2024-001",
                    nombreAsegurado = "Juan Carlos Pérez González",
                    tipoSeguro = "Vehículo",
                    estado = "Activo",
                    fechaInicio = DateTime.Now.AddDays(-30),
                    fechaVencimiento = DateTime.Now.AddDays(335),
                    prima = 25000.00m,
                    moneda = "CRC",
                    aseguradora = "INS",
                    placa = "SJJ-123"
                },
                new
                {
                    id = 2,
                    numeroPoliza = "POL-2024-002",
                    nombreAsegurado = "María Elena Rodríguez Vargas",
                    tipoSeguro = "Hogar",
                    estado = "Activo",
                    fechaInicio = DateTime.Now.AddDays(-25),
                    fechaVencimiento = DateTime.Now.AddDays(340),
                    prima = 15000.00m,
                    moneda = "CRC",
                    aseguradora = "Mapfre",
                    placa = ""
                },
                new
                {
                    id = 3,
                    numeroPoliza = "POL-2024-003",
                    nombreAsegurado = "Roberto Andrés Jiménez Castro",
                    tipoSeguro = "Vehículo",
                    estado = "Pendiente",
                    fechaInicio = DateTime.Now.AddDays(-20),
                    fechaVencimiento = DateTime.Now.AddDays(345),
                    prima = 30000.00m,
                    moneda = "CRC",
                    aseguradora = "INS",
                    placa = "GAM-456"
                },
                new
                {
                    id = 4,
                    numeroPoliza = "POL-2024-004",
                    nombreAsegurado = "Ana Patricia Morales Hernández",
                    tipoSeguro = "Vida",
                    estado = "Activo",
                    fechaInicio = DateTime.Now.AddDays(-15),
                    fechaVencimiento = DateTime.Now.AddDays(350),
                    prima = 12000.00m,
                    moneda = "CRC",
                    aseguradora = "BCR Seguros",
                    placa = ""
                },
                new
                {
                    id = 5,
                    numeroPoliza = "POL-2024-005",
                    nombreAsegurado = "Carlos Eduardo González Méndez",
                    tipoSeguro = "Vehículo",
                    estado = "Vencido",
                    fechaInicio = DateTime.Now.AddDays(-400),
                    fechaVencimiento = DateTime.Now.AddDays(-35),
                    prima = 28000.00m,
                    moneda = "CRC",
                    aseguradora = "Mapfre",
                    placa = "CAR-789"
                }
            };
        }
    }
}