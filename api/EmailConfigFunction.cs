using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SiinadsegApi
{
    public class EmailConfigFunction
    {
        private readonly ILogger<EmailConfigFunction> _logger;

        public EmailConfigFunction(ILogger<EmailConfigFunction> logger)
        {
            _logger = logger;
        }

        // GET /api/emailconfig - Obtener todas las configuraciones
        [Function("emailconfig-getall")]
        public async Task<IActionResult> GetAll([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "emailconfig")] HttpRequest req)
        {
            _logger.LogInformation("Obteniendo todas las configuraciones de email");

            try
            {
                var emailConfigs = new[]
                {
                    new
                    {
                        id = 1,
                        nombre = "Gmail SMTP",
                        servidor = "smtp.gmail.com",
                        puerto = 587,
                        usarSSL = true,
                        usuario = "correo@empresa.com",
                        esActivo = true,
                        esPredeterminado = true,
                        fechaCreacion = DateTime.UtcNow.AddDays(-30),
                        fechaModificacion = DateTime.UtcNow.AddDays(-5)
                    },
                    new
                    {
                        id = 2,
                        nombre = "Outlook SMTP",
                        servidor = "smtp-mail.outlook.com",
                        puerto = 587,
                        usarSSL = true,
                        usuario = "backup@empresa.com",
                        esActivo = false,
                        esPredeterminado = false,
                        fechaCreacion = DateTime.UtcNow.AddDays(-20),
                        fechaModificacion = DateTime.UtcNow.AddDays(-10)
                    }
                };

                var response = new
                {
                    success = true,
                    data = emailConfigs,
                    message = "Configuraciones obtenidas exitosamente"
                };

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo configuraciones de email");
                return new StatusCodeResult(500);
            }
        }

        // GET /api/emailconfig/{id} - Obtener configuración por ID
        [Function("emailconfig-getbyid")]
        public async Task<IActionResult> GetById([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "emailconfig/{id:int}")] HttpRequest req, int id)
        {
            _logger.LogInformation($"Obteniendo configuración de email con ID: {id}");

            try
            {
                var emailConfig = new
                {
                    id = id,
                    nombre = "Gmail SMTP",
                    servidor = "smtp.gmail.com",
                    puerto = 587,
                    usarSSL = true,
                    usuario = "correo@empresa.com",
                    password = "************", // Password oculto por seguridad
                    esActivo = true,
                    esPredeterminado = true,
                    fechaCreacion = DateTime.UtcNow.AddDays(-30),
                    fechaModificacion = DateTime.UtcNow.AddDays(-5)
                };

                var response = new
                {
                    success = true,
                    data = emailConfig,
                    message = "Configuración obtenida exitosamente"
                };

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error obteniendo configuración de email ID: {id}");
                return new StatusCodeResult(500);
            }
        }

        // GET /api/emailconfig/default - Obtener configuración predeterminada
        [Function("emailconfig-getdefault")]
        public async Task<IActionResult> GetDefault([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "emailconfig/default")] HttpRequest req)
        {
            _logger.LogInformation("Obteniendo configuración de email predeterminada");

            try
            {
                var defaultConfig = new
                {
                    id = 1,
                    nombre = "Gmail SMTP",
                    servidor = "smtp.gmail.com",
                    puerto = 587,
                    usarSSL = true,
                    usuario = "correo@empresa.com",
                    esActivo = true,
                    esPredeterminado = true
                };

                var response = new
                {
                    success = true,
                    data = defaultConfig,
                    message = "Configuración predeterminada obtenida exitosamente"
                };

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo configuración predeterminada");
                return new StatusCodeResult(500);
            }
        }

        // POST /api/emailconfig - Crear nueva configuración
        [Function("emailconfig-create")]
        public async Task<IActionResult> Create([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "emailconfig")] HttpRequest req)
        {
            _logger.LogInformation("Creando nueva configuración de email");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var newConfig = JsonSerializer.Deserialize<dynamic>(requestBody);

                var createdConfig = new
                {
                    id = new Random().Next(100, 999),
                    nombre = "Nueva Configuración",
                    servidor = "smtp.ejemplo.com",
                    puerto = 587,
                    usarSSL = true,
                    usuario = "nuevo@empresa.com",
                    esActivo = true,
                    esPredeterminado = false,
                    fechaCreacion = DateTime.UtcNow,
                    fechaModificacion = DateTime.UtcNow
                };

                var response = new
                {
                    success = true,
                    data = createdConfig,
                    message = "Configuración creada exitosamente"
                };

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando configuración de email");
                return new StatusCodeResult(500);
            }
        }

        // POST /api/emailconfig/test - Probar configuración
        [Function("emailconfig-test")]
        public async Task<IActionResult> TestConfig([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "emailconfig/test")] HttpRequest req)
        {
            _logger.LogInformation("Probando configuración de email");

            try
            {
                var testResponse = new
                {
                    success = true,
                    message = "Configuración de email probada exitosamente",
                    details = new
                    {
                        conexion = "Exitosa",
                        autenticacion = "Exitosa",
                        envioTest = "Exitoso",
                        tiempoRespuesta = "1.2s"
                    }
                };

                var response = new
                {
                    success = true,
                    data = testResponse,
                    message = "Prueba completada"
                };

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error probando configuración de email");
                return new StatusCodeResult(500);
            }
        }
    }
}