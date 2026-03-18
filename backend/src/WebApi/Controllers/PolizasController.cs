using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Services;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PolizasController : ControllerBase
    {
        private readonly IPolizaService _polizaService;
        private readonly IExcelService _excelService;
        private readonly ILogger<PolizasController> _logger;

        public PolizasController(IPolizaService polizaService, IExcelService excelService, ILogger<PolizasController> logger)
        {
            _polizaService = polizaService;
            _excelService = excelService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todas las pólizas activas
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PolizaDto>))]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var polizas = await _polizaService.GetAllAsync();
                return Ok(polizas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo pólizas");
                return BadRequest(new { message = "Error obteniendo pólizas" });
            }
        }

        /// <summary>
        /// Obtener pólizas por perfil
        /// </summary>
        [HttpGet("perfil/{perfilId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PolizaDto>))]
        public async Task<IActionResult> GetByPerfil(int perfilId)
        {
            try
            {
                var polizas = await _polizaService.GetByPerfilIdAsync(perfilId);
                return Ok(polizas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo pólizas por perfil {PerfilId}", perfilId);
                return BadRequest(new { message = "Error obteniendo pólizas" });
            }
        }

        /// <summary>
        /// Obtener póliza por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolizaDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var poliza = await _polizaService.GetByIdAsync(id);
                if (poliza == null)
                    return NotFound(new { message = "Póliza no encontrada" });

                return Ok(poliza);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo póliza {Id}", id);
                return BadRequest(new { message = "Error obteniendo póliza" });
            }
        }

        /// <summary>
        /// Buscar póliza por número
        /// </summary>
        [HttpGet("numero/{numeroPoliza}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolizaDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByNumero(string numeroPoliza)
        {
            try
            {
                var poliza = await _polizaService.GetByNumeroPolizaAsync(numeroPoliza);
                if (poliza == null)
                    return NotFound(new { message = "Póliza no encontrada" });

                return Ok(poliza);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo póliza por número {NumeroPoliza}", numeroPoliza);
                return BadRequest(new { message = "Error obteniendo póliza" });
            }
        }

        /// <summary>
        /// Buscar pólizas por término (número o nombre)
        /// </summary>
        [HttpGet("buscar")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PolizaDto>))]
        public async Task<IActionResult> Buscar([FromQuery] string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                    return Ok(Array.Empty<PolizaDto>());

                var polizas = await _polizaService.GetAllAsync();
                var resultado = polizas.Where(p =>
                    p.NumeroPoliza.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                    p.NombreAsegurado.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                    p.NumeroCedula.Contains(termino, StringComparison.OrdinalIgnoreCase)
                ).Take(10).ToList();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buscando pólizas con término {Termino}", termino);
                return BadRequest(new { message = "Error buscando pólizas" });
            }
        }

        /// <summary>
        /// Obtener pólizas por aseguradora
        /// </summary>
        [HttpGet("aseguradora/{aseguradora}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PolizaDto>))]
        public async Task<IActionResult> GetByAseguradora(string aseguradora)
        {
            try
            {
                var polizas = await _polizaService.GetByAseguradoraAsync(aseguradora);
                return Ok(polizas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo pólizas por aseguradora {Aseguradora}", aseguradora);
                return BadRequest(new { message = "Error obteniendo pólizas" });
            }
        }

        /// <summary>
        /// Crear nueva póliza
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,DataLoader")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PolizaDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreatePolizaDto dto)
        {
            try
            {
                var poliza = await _polizaService.CreateAsync(dto);
                _logger.LogInformation("Póliza creada: {NumeroPoliza} por usuario {UserId}", dto.NumeroPoliza, GetCurrentUserId());
                return CreatedAtAction(nameof(GetById), new { id = poliza.Id }, poliza);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando póliza");
                return BadRequest(new { message = "Error creando póliza" });
            }
        }

        /// <summary>
        /// Actualizar póliza existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,DataLoader")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolizaDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] CreatePolizaDto dto)
        {
            try
            {
                var poliza = await _polizaService.UpdateAsync(id, dto);
                _logger.LogInformation("Póliza {Id} actualizada por usuario {UserId}", id, GetCurrentUserId());
                return Ok(poliza);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Póliza no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando póliza {Id}", id);
                return BadRequest(new { message = "Error actualizando póliza" });
            }
        }

        /// <summary>
        /// Eliminar póliza
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,DataLoader")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _polizaService.DeleteAsync(id);
                _logger.LogInformation("Póliza {Id} eliminada por usuario {UserId}", id, GetCurrentUserId());
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Póliza no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando póliza {Id}", id);
                return BadRequest(new { message = "Error eliminando póliza" });
            }
        }

        /// <summary>
        /// Cargar pólizas desde archivo Excel
        /// </summary>
        [HttpPost("upload")]
        [Authorize(Roles = "Admin,DataLoader")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DataUploadResultDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadExcel([FromForm] int perfilId, [FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Archivo no proporcionado" });

                // Validar extensión
                var allowedExtensions = new[] { ".xlsx", ".xls" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new { message = "Solo se permiten archivos Excel (.xlsx, .xls)" });

                // Validar tamaño (10MB máximo)
                if (file.Length > 10 * 1024 * 1024)
                    return BadRequest(new { message = "El archivo excede el tamaño máximo de 10MB" });

                var userId = GetCurrentUserId();
                var result = await _polizaService.ProcesarExcelPolizasAsync(perfilId, file, int.Parse(userId));
                
                _logger.LogInformation("Archivo Excel de pólizas procesado: {FileName}, {ProcessedRecords} registros procesados por usuario {UserId}", 
                    file.FileName, result.ProcessedRecords, userId);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando archivo Excel de pólizas");
                return BadRequest(new { message = "Error procesando archivo Excel" });
            }
        }

        /// <summary>
        /// Descargar template Excel para cargar pólizas
        /// </summary>
        [HttpGet("template")]
        [Authorize(Roles = "Admin,DataLoader")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadTemplate()
        {
            try
            {
                // Datos de ejemplo para el template con el nuevo formato
                var templateData = new[]
                {
                    new
                    {
                        Poliza = "POL-2024-001",
                        Nombre = "Juan Pérez García",
                        NumeroCedula = "1-2345-6789",
                        Prima = "150000",
                        Moneda = "CRC",
                        Fecha = "2024-12-31",
                        Frecuencia = "Mensual",
                        Aseguradora = "Instituto Nacional de Seguros (INS)",
                        Placa = "ABC123",
                        Marca = "Toyota",
                        Modelo = "Corolla",
                        Año = "2023",
                        Correo = "juan.perez@email.com",
                        NumeroTelefono = "+506 8888-9999"
                    }
                };

                var headers = new[]
                {
                    "POLIZA",
                    "NOMBRE",
                    "NUMEROCEDULA",
                    "PRIMA",
                    "MONEDA",
                    "FECHA",
                    "FRECUENCIA",
                    "ASEGURADORA",
                    "PLACA",
                    "MARCA",
                    "MODELO",
                    "AÑO",
                    "CORREO",
                    "NUMEROTELEFONO"
                };

                var excelBytes = await _excelService.GenerateExcelAsync(
                    templateData,
                    headers,
                    item => new object[]
                    {
                        item.Poliza,
                        item.Nombre,
                        item.NumeroCedula,
                        item.Prima,
                        item.Moneda,
                        item.Fecha,
                        item.Frecuencia,
                        item.Aseguradora,
                        item.Placa,
                        item.Marca,
                        item.Modelo,
                        item.Año,
                        item.Correo,
                        item.NumeroTelefono
                    }
                );

                var fileName = $"template_polizas_{DateTime.Now:yyyyMMdd}.xlsx";
                
                _logger.LogInformation("Template de pólizas descargado por usuario {UserId}", GetCurrentUserId());

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando template de pólizas");
                return StatusCode(500, new { message = "Error generando template" });
            }
        }

        /// <summary>
        /// Genera un archivo Excel corregible a partir de los registros rechazados en un upload previo.
        /// El archivo tiene las mismas columnas que el archivo original más una columna MOTIVO_ERROR al final,
        /// de modo que el usuario pueda corregir y resubir directamente.
        /// </summary>
        [HttpPost("errors-excel")]
        [Authorize(Roles = "Admin,DataLoader")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DownloadErrorsExcel([FromBody] ErrorsExcelRequestDto request)
        {
            if (request.FailedRecords == null || request.FailedRecords.Count == 0)
                return BadRequest(new { message = "No hay registros con errores para generar el archivo." });

            try
            {
                // Column headers: original file columns + trailing error column
                var outputHeaders = request.FileHeaders
                    .Select(h => h.ToUpperInvariant())
                    .Append("MOTIVO_ERROR")
                    .ToArray();

                var excelBytes = await _excelService.GenerateExcelAsync(
                    request.FailedRecords,
                    outputHeaders,
                    record =>
                    {
                        var values = request.FileHeaders
                            .Select(h => (object)(record.OriginalData.TryGetValue(h, out var val) ? val : string.Empty))
                            .ToList();
                        values.Add(record.Error);
                        return values.ToArray();
                    });

                var baseName = Path.GetFileNameWithoutExtension(request.OriginalFileName ?? "polizas");
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"ERRORES_{baseName}_{timestamp}_{request.FailedRecords.Count}reg.xlsx";

                _logger.LogInformation(
                    "Archivo de errores generado: {FileName}, {ErrorCount} registros, solicitado por usuario {UserId}",
                    fileName, request.FailedRecords.Count, GetCurrentUserId());

                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando archivo Excel de errores");
                return StatusCode(500, new { message = "Error generando archivo de errores" });
            }
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
        }
    }
}