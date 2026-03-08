using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SiinadsegApi
{
    public class ConfiguracionFunction
    {
        private readonly ILogger<ConfiguracionFunction> _logger;

        public ConfiguracionFunction(ILogger<ConfiguracionFunction> logger)
        {
            _logger = logger;
        }

        [Function("configuracion")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            _logger.LogInformation("Obteniendo configuraciones de la aplicación");

            try
            {
                var configuraciones = new
                {
                    aplicacion = new
                    {
                        nombre = "SIINADSEG",
                        version = "1.0.0",
                        ambiente = "Azure Static Web App"
                    },
                    caracteristicas = new
                    {
                        usuarios = true,
                        polizas = true,
                        cobros = true,
                        reclamos = true,
                        emails = true
                    },
                    limites = new
                    {
                        maxUploadSize = 10485760, // 10MB
                        maxRegistrosPorPagina = 50
                    },
                    estado = "activo",
                    timestamp = DateTime.UtcNow
                };

                return new OkObjectResult(configuraciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo configuraciones");
                return new StatusCodeResult(500);
            }
        }
    }
}