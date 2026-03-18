using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;

namespace Application.Services
{
    public class PolizaService : IPolizaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PolizaService> _logger;

        // ── Validation rule sets ────────────────────────────────────────────────
        private static readonly IReadOnlySet<string> ValidCurrencies =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CRC", "USD", "EUR" };

        private static readonly IReadOnlySet<string> ValidFrecuencias =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "MENSUAL", "TRIMESTRAL", "SEMESTRAL", "ANUAL", "BIMESTRAL", "SEMANAL", "QUINCENAL" };

        private static readonly string[] DateFormats =
            { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "yyyy/MM/dd" };

        public PolizaService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PolizaService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<PolizaDto>> GetAllAsync()
        {
            var polizas = await _unitOfWork.Polizas.GetActivasAsync();
            return _mapper.Map<IEnumerable<PolizaDto>>(polizas);
        }

        public async Task<IEnumerable<PolizaDto>> GetByPerfilIdAsync(int perfilId)
        {
            var polizas = await _unitOfWork.Polizas.GetByPerfilIdAsync(perfilId);
            return _mapper.Map<IEnumerable<PolizaDto>>(polizas);
        }

        public async Task<PolizaDto?> GetByIdAsync(int id)
        {
            var poliza = await _unitOfWork.Polizas.GetByIdAsync(id);
            return poliza != null ? _mapper.Map<PolizaDto>(poliza) : null;
        }

        public async Task<PolizaDto?> GetByNumeroPolizaAsync(string numeroPoliza)
        {
            var poliza = await _unitOfWork.Polizas.GetByNumeroPolizaAsync(numeroPoliza);
            return poliza != null ? _mapper.Map<PolizaDto>(poliza) : null;
        }

        public async Task<IEnumerable<PolizaDto>> GetByAseguradoraAsync(string aseguradora)
        {
            var polizas = await _unitOfWork.Polizas.GetByAseguradoraAsync(aseguradora);
            return _mapper.Map<IEnumerable<PolizaDto>>(polizas);
        }

        public async Task<PolizaDto> CreateAsync(CreatePolizaDto dto)
        {
            _logger.LogDebug("[PolizaService.CreateAsync] IN → NumeroPoliza={NumeroPoliza}, NombreAsegurado={NombreAsegurado}, Prima={Prima}, Moneda={Moneda}, Frecuencia={Frecuencia}, Aseguradora={Aseguradora}, FechaVigencia={FechaVigencia}",
                dto.NumeroPoliza, dto.NombreAsegurado, dto.Prima, dto.Moneda, dto.Frecuencia, dto.Aseguradora, dto.FechaVigencia);

            var existingPoliza = await _unitOfWork.Polizas.GetByNumeroPolizaAsync(dto.NumeroPoliza);
            if (existingPoliza != null)
                throw new InvalidOperationException("Ya existe una póliza con este número");

            var poliza = _mapper.Map<Poliza>(dto);
            poliza.EsActivo  = true;
            poliza.CreatedAt = DateTime.UtcNow;
            poliza.UpdatedAt = DateTime.UtcNow;

            _logger.LogDebug("[PolizaService.CreateAsync] OUT → NumeroPoliza={NumeroPoliza}, NombreAsegurado={NombreAsegurado}, Prima={Prima}, Frecuencia={Frecuencia}, Aseguradora={Aseguradora}, FechaVigencia={FechaVigencia}",
                poliza.NumeroPoliza, poliza.NombreAsegurado, poliza.Prima, poliza.Frecuencia, poliza.Aseguradora, poliza.FechaVigencia.ToString("dd-MM-yyyy"));

            await _unitOfWork.Polizas.AddAsync(poliza);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PolizaDto>(poliza);
        }

        public async Task<PolizaDto> UpdateAsync(int id, CreatePolizaDto dto)
        {
            _logger.LogDebug("[PolizaService.UpdateAsync] IN → Id={Id}, NumeroPoliza={NumeroPoliza}, NombreAsegurado={NombreAsegurado}, Prima={Prima}, Moneda={Moneda}, Frecuencia={Frecuencia}, Aseguradora={Aseguradora}, FechaVigencia={FechaVigencia}",
                id, dto.NumeroPoliza, dto.NombreAsegurado, dto.Prima, dto.Moneda, dto.Frecuencia, dto.Aseguradora, dto.FechaVigencia);

            var poliza = await _unitOfWork.Polizas.GetByIdAsync(id);
            if (poliza == null)
                throw new KeyNotFoundException("Póliza no encontrada");

            _mapper.Map(dto, poliza);
            poliza.UpdatedAt = DateTime.UtcNow;

            _logger.LogDebug("[PolizaService.UpdateAsync] OUT → NumeroPoliza={NumeroPoliza}, NombreAsegurado={NombreAsegurado}, Prima={Prima}, Frecuencia={Frecuencia}, Aseguradora={Aseguradora}, FechaVigencia={FechaVigencia}",
                poliza.NumeroPoliza, poliza.NombreAsegurado, poliza.Prima, poliza.Frecuencia, poliza.Aseguradora, poliza.FechaVigencia.ToString("dd-MM-yyyy"));

            await _unitOfWork.Polizas.UpdateAsync(poliza);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PolizaDto>(poliza);
        }

        public async Task DeleteAsync(int id)
        {
            var poliza = await _unitOfWork.Polizas.GetByIdAsync(id);
            if (poliza == null)
                throw new KeyNotFoundException("Póliza no encontrada");

            poliza.IsDeleted = true;
            poliza.EsActivo = false;
            poliza.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.Polizas.UpdateAsync(poliza);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<DataUploadResultDto> ProcesarExcelPolizasAsync(int perfilId, IFormFile file, int userId)
        {
            var result = new DataUploadResultDto();
            var errors = new List<string>();
            var polizas = new List<Poliza>();

            // Crear registro de carga
            var dataRecord = new DataRecord
            {
                FileName = file.FileName,
                FileType = file.ContentType,
                FileSize = file.Length,
                UploadedByUserId = userId,
                PerfilId = perfilId,
                Status = "Processing",
                ProcessedAt = DateTime.UtcNow
            };

            await _unitOfWork.DataRecords.AddAsync(dataRecord);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                
                if (workbook.Worksheets.Count == 0)
                {
                    throw new InvalidOperationException("El archivo Excel no contiene hojas de trabajo.");
                }
                
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1); // Saltar header
                result.TotalRecords = rows.Count();

                // Leer la fila de encabezados para mapeo dinámico de columnas
                var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                var headerRow = worksheet.Row(1);
                foreach (var cell in headerRow.CellsUsed())
                {
                    var h = cell.GetString().Trim();
                    if (!string.IsNullOrEmpty(h))
                        columnMap[h.ToUpperInvariant()] = cell.Address.ColumnNumber;
                }
                _logger.LogDebug("Columnas detectadas en archivo Excel: {Columns}", string.Join(", ", columnMap.Keys));

                // Alias resolution: map common alternative header names to canonical keys
                var nombreAliases = new[] { "NOMBRE ASEGURADO", "NOMBRE_ASEGURADO", "ASEGURADO", "TITULAR", "NOMBRE COMPLETO", "CLIENTE" };
                if (!columnMap.ContainsKey("NOMBRE"))
                {
                    foreach (var alias in nombreAliases)
                        if (columnMap.TryGetValue(alias, out var aliasCol)) { columnMap["NOMBRE"] = aliasCol; break; }
                }

                // Preserve ordered file headers for the errors-download feature
                result.FileHeaders = columnMap
                    .OrderBy(kvp => kvp.Value)
                    .Select(kvp => kvp.Key)
                    .ToList();

                // Fallback positions depend on whether a MOD column is present in the file
                int nombreFallback = columnMap.ContainsKey("MOD") ? 3 : 2;

                if (result.TotalRecords == 0)
                {
                    errors.Add("El archivo Excel no contiene datos o solo tiene encabezados.");
                    result.ErrorRecords = 0;
                    result.ProcessedRecords = 0;
                }
                else
                {
                    _logger.LogInformation("Iniciando procesamiento de {TotalRecords} filas del archivo Excel", result.TotalRecords);
                    foreach (var row in rows)
                    {
                        _logger.LogDebug("Procesando fila {RowNumber}", row.RowNumber());
                        try
                        {
                            // ── Step 1: Capture all original cell values ────────────────────────
                            var rowData = new Dictionary<string, string>
                            {
                                ["POLIZA"]         = GetCell(row, columnMap, "POLIZA",         1),
                                ["MOD"]            = GetCell(row, columnMap, "MOD",            -1, true),
                                ["NOMBRE"]         = GetCell(row, columnMap, "NOMBRE",         nombreFallback),
                                ["NUMEROCEDULA"]   = GetCell(row, columnMap, "NUMEROCEDULA",   -1, true),
                                ["PRIMA"]          = GetCell(row, columnMap, "PRIMA",          4),
                                ["MONEDA"]         = GetCell(row, columnMap, "MONEDA",         5),
                                ["FECHA"]          = GetCell(row, columnMap, "FECHA",          6),
                                ["FRECUENCIA"]     = GetCell(row, columnMap, "FRECUENCIA",     7),
                                ["ASEGURADORA"]    = GetCell(row, columnMap, "ASEGURADORA",    8),
                                ["PLACA"]          = GetCell(row, columnMap, "PLACA",          9,  true),
                                ["MARCA"]          = GetCell(row, columnMap, "MARCA",          10, true),
                                ["MODELO"]         = GetCell(row, columnMap, "MODELO",         11, true),
                                ["AÑO"]            = GetCell(row, columnMap, "AÑO",            12, true),
                                ["CORREO"]         = GetCell(row, columnMap, "CORREO",         -1, true),
                                ["NUMEROTELEFONO"] = GetCell(row, columnMap, "NUMEROTELEFONO", -1, true),
                                ["OBSERVACIONES"]  = GetCell(row, columnMap, "OBSERVACIONES",  -1, true),
                                ["COLECTIVA"]      = GetCell(row, columnMap, "COLECTIVA",      -1, true),
                            };

                            // ── Step 2: Strict validation (reject before touching DB) ───────────
                            var validationErrors = ValidatePolizaRow(rowData, row.RowNumber());
                            if (validationErrors.Any())
                            {
                                var errorMsg = string.Join("; ", validationErrors);
                                errors.Add($"Fila {row.RowNumber()}: {errorMsg}");
                                result.FailedRecords.Add(new FailedRecordDto
                                {
                                    RowNumber    = row.RowNumber(),
                                    Error        = errorMsg,
                                    OriginalData = rowData
                                });
                                result.ErrorRecords++;
                                _logger.LogWarning("Fila {RowNumber}: rechazada ({ErrorCount} error(es)) — {Errors}",
                                    row.RowNumber(), validationErrors.Count, errorMsg);
                                continue;
                            }

                            // ── Step 3: Build entity (only reached when validation passes) ──────
                            var poliza = new Poliza
                            {
                                NumeroPoliza    = TruncateString(rowData["POLIZA"],        50,  row.RowNumber(), "POLIZA"),
                                Modalidad       = TruncateString(rowData["MOD"],           50,  row.RowNumber(), "MOD"),
                                NombreAsegurado = TruncateString(rowData["NOMBRE"],        200, row.RowNumber(), "NOMBRE"),
                                NumeroCedula    = TruncateString(rowData["NUMEROCEDULA"],  50,  row.RowNumber(), "NUMEROCEDULA"),
                                Prima           = ParseDecimal(rowData["PRIMA"]),
                                Moneda          = rowData["MONEDA"].Trim().ToUpperInvariant(),
                                FechaVigencia   = ParseDate(rowData["FECHA"]),
                                Frecuencia      = TruncateString(rowData["FRECUENCIA"].Trim().ToUpperInvariant(), 50,  row.RowNumber(), "FRECUENCIA"),
                                Aseguradora     = TruncateString(rowData["ASEGURADORA"],   100, row.RowNumber(), "ASEGURADORA"),
                                Placa           = TruncateString(rowData["PLACA"],         8,   row.RowNumber(), "PLACA"),
                                Marca           = TruncateString(rowData["MARCA"],         50,  row.RowNumber(), "MARCA"),
                                Modelo          = TruncateString(rowData["MODELO"],        50,  row.RowNumber(), "MODELO"),
                                Año             = TruncateString(rowData["AÑO"],           4,   row.RowNumber(), "AÑO"),
                                Correo          = TruncateString(rowData["CORREO"],        100, row.RowNumber(), "CORREO"),
                                NumeroTelefono  = TruncateString(rowData["NUMEROTELEFONO"],20,  row.RowNumber(), "NUMEROTELEFONO"),
                                Observaciones   = TruncateString(rowData["OBSERVACIONES"], 500, row.RowNumber(), "OBSERVACIONES"),
                                PerfilId        = perfilId,
                                CreatedBy       = userId.ToString()
                            };

                            // ── Step 4: Upsert / duplicate check ────────────────────────────────
                            _logger.LogDebug("Fila {RowNumber}: datos válidos, verificando duplicados", row.RowNumber());

                            var existingPoliza = await _unitOfWork.Polizas.GetByNumeroPolizaAsync(poliza.NumeroPoliza);
                            if (existingPoliza != null)
                            {
                                // UPSERT: update existing record with fresh data from the file
                                existingPoliza.Modalidad       = poliza.Modalidad;
                                existingPoliza.NombreAsegurado = poliza.NombreAsegurado;
                                existingPoliza.NumeroCedula    = poliza.NumeroCedula;
                                existingPoliza.Prima           = poliza.Prima;
                                existingPoliza.Moneda          = poliza.Moneda;
                                existingPoliza.FechaVigencia   = poliza.FechaVigencia;
                                existingPoliza.Frecuencia      = poliza.Frecuencia;
                                existingPoliza.Aseguradora     = poliza.Aseguradora;
                                existingPoliza.Placa           = poliza.Placa;
                                existingPoliza.Marca           = poliza.Marca;
                                existingPoliza.Modelo          = poliza.Modelo;
                                existingPoliza.Año             = poliza.Año;
                                existingPoliza.Correo          = poliza.Correo;
                                existingPoliza.NumeroTelefono  = poliza.NumeroTelefono;
                                existingPoliza.Observaciones   = poliza.Observaciones;
                                await _unitOfWork.Polizas.UpdateAsync(existingPoliza);
                                await _unitOfWork.SaveChangesAsync();
                                result.ProcessedRecords++;
                                _logger.LogInformation("Fila {RowNumber}: póliza {NumeroPoliza} actualizada (upsert)", row.RowNumber(), poliza.NumeroPoliza);
                                continue;
                            }

                            var duplicateInBatch = polizas.FirstOrDefault(p => p.NumeroPoliza == poliza.NumeroPoliza);
                            if (duplicateInBatch != null)
                            {
                                var dupError = $"Número de póliza '{poliza.NumeroPoliza}' duplicado en el mismo archivo";
                                errors.Add($"Fila {row.RowNumber()}: {dupError}");
                                result.FailedRecords.Add(new FailedRecordDto
                                {
                                    RowNumber    = row.RowNumber(),
                                    Error        = dupError,
                                    OriginalData = rowData
                                });
                                result.ErrorRecords++;
                                continue;
                            }

                            polizas.Add(poliza);
                            result.ProcessedRecords++;
                            _logger.LogDebug("Fila {RowNumber}: registro aceptado. Total aceptados: {ProcessedRecords}", row.RowNumber(), result.ProcessedRecords);
                        }
                        catch (Exception ex)
                        {
                            var errorMsg = ex.Message;
                            errors.Add($"Fila {row.RowNumber()}: {errorMsg}");

                            // Best-effort capture of original data when an unexpected exception occurs
                            var fallbackData = new Dictionary<string, string>();
                            try
                            {
                                foreach (var kvp in columnMap)
                                    fallbackData[kvp.Key] = GetCellValueSafe(row, kvp.Value, kvp.Key, optional: true);
                            }
                            catch
                            {
                                fallbackData["Error"] = "No se pudieron extraer los datos originales";
                            }

                            result.FailedRecords.Add(new FailedRecordDto
                            {
                                RowNumber    = row.RowNumber(),
                                Error        = errorMsg,
                                OriginalData = fallbackData
                            });
                            result.ErrorRecords++;
                            _logger.LogWarning(ex, "Error inesperado en fila {RowNumber}: {ErrorMessage}", row.RowNumber(), ex.Message);
                        }
                    }

                _logger.LogInformation("Resumen procesamiento Excel: {ProcessedRecords} válidos, {ErrorRecords} errores de {TotalRecords} total",
                    result.ProcessedRecords, result.ErrorRecords, result.TotalRecords);

                // Guardar registros válidos (si los hay)
                if (polizas.Any())
                {
                    try
                    {
                        await _unitOfWork.Polizas.AddRangeAsync(polizas);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    catch (Exception saveEx)
                    {
                        // Si falla el guardado, ajustar los contadores
                        result.ErrorRecords += result.ProcessedRecords;
                        result.ProcessedRecords = 0;
                        errors.Add($"Error al guardar en base de datos: {saveEx.Message}");
                    }
                }

                // Actualizar registro de carga
                dataRecord.TotalRecords = result.TotalRecords;
                dataRecord.ProcessedRecords = result.ProcessedRecords;
                dataRecord.ErrorRecords = result.ErrorRecords;
                
                // Determinar estado basado en resultados
                if (result.ProcessedRecords > 0 && result.ErrorRecords == 0)
                {
                    dataRecord.Status = "Completed";
                }
                else if (result.ProcessedRecords > 0 && result.ErrorRecords > 0)
                {
                    dataRecord.Status = "Completed with errors";
                }
                else if (result.ProcessedRecords == 0 && result.ErrorRecords > 0)
                {
                    dataRecord.Status = "Failed";
                }
                else
                {
                    dataRecord.Status = "No data";
                }
                
                dataRecord.ErrorDetails = string.Join(";", errors);
                
                await _unitOfWork.DataRecords.UpdateAsync(dataRecord);
                await _unitOfWork.SaveChangesAsync();

                result.Status = dataRecord.Status;
                result.Errors = errors;
                
                // Generar mensaje descriptivo con resumen de errores
                if (result.ProcessedRecords > 0 && result.ErrorRecords == 0)
                {
                    result.Message = $"✅ Archivo procesado exitosamente. {result.ProcessedRecords} pólizas importadas sin errores.";
                }
                else if (result.ProcessedRecords > 0 && result.ErrorRecords > 0)
                {
                    var errorSummary = GenerateErrorSummary(errors);
                    result.Message = $"⚠️ Archivo procesado parcialmente. {result.ProcessedRecords} pólizas importadas correctamente, {result.ErrorRecords} registros con errores fueron omitidos.\n\nErrores encontrados:\n{errorSummary}";
                }
                else if (result.ProcessedRecords == 0 && result.ErrorRecords > 0)
                {
                    var errorSummary = GenerateErrorSummary(errors);
                    result.Message = $"❌ No se pudo importar ninguna póliza. {result.ErrorRecords} errores encontrados.\n\nTodos los registros tienen errores:\n{errorSummary}\n\nPor favor corrija el archivo y vuelva a intentar.";
                }
                else
                {
                    result.Message = "ℹ️ El archivo no contenía registros válidos para procesar.";
                }
                } // Cierre del bloque else que contiene el foreach
            } // Cierre del try principal
            catch (Exception ex)
            {
                dataRecord.Status = "Failed";
                dataRecord.ErrorDetails = ex.Message;
                await _unitOfWork.DataRecords.UpdateAsync(dataRecord);
                await _unitOfWork.SaveChangesAsync();

                throw new InvalidOperationException($"Error procesando archivo Excel: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Validates the required fields and format rules for a single Excel row.
        /// Returns an empty list when the row is valid; otherwise returns one message per failing rule.
        /// </summary>
        private static List<string> ValidatePolizaRow(Dictionary<string, string> rowData, int rowNumber)
        {
            var errors = new List<string>();

            // POLIZA — required
            if (!rowData.TryGetValue("POLIZA", out var poliza) || string.IsNullOrWhiteSpace(poliza))
                errors.Add("POLIZA es obligatoria");

            // NOMBRE — required
            if (!rowData.TryGetValue("NOMBRE", out var nombre) || string.IsNullOrWhiteSpace(nombre))
                errors.Add("NOMBRE es obligatorio");

            // PRIMA — required, must be a positive decimal
            rowData.TryGetValue("PRIMA", out var primaRaw);
            if (string.IsNullOrWhiteSpace(primaRaw))
            {
                errors.Add("PRIMA es obligatoria");
            }
            else
            {
                // Re-use the same normalization logic as ParseDecimal
                var cleanedPrima = primaRaw.Replace("$", "").Replace(" ", "").Trim();
                if (cleanedPrima.Contains(','))
                    cleanedPrima = cleanedPrima.Replace(".", "").Replace(",", ".");

                if (!decimal.TryParse(cleanedPrima, NumberStyles.Number, CultureInfo.InvariantCulture, out var prima)
                    || prima <= 0)
                    errors.Add($"PRIMA '{primaRaw}' inválida — debe ser un número positivo (ej. 1250.00 o 1.250,00)");
            }

            // MONEDA — required, CRC | USD | EUR
            rowData.TryGetValue("MONEDA", out var moneda);
            if (string.IsNullOrWhiteSpace(moneda))
                errors.Add("MONEDA es obligatoria");
            else if (!ValidCurrencies.Contains(moneda.Trim()))
                errors.Add($"MONEDA '{moneda}' no es válida — valores permitidos: CRC, USD, EUR");

            // FECHA — required, parseable date
            rowData.TryGetValue("FECHA", out var fecha);
            if (string.IsNullOrWhiteSpace(fecha))
            {
                errors.Add("FECHA es obligatoria");
            }
            else if (!DateTime.TryParseExact(fecha.Trim(), DateFormats,
                         CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                     && !DateTime.TryParse(fecha.Trim(), out _))
            {
                errors.Add($"FECHA '{fecha}' formato inválido — use DD/MM/AAAA (ej. 31/12/2025)");
            }

            // FRECUENCIA — required, allowed values
            rowData.TryGetValue("FRECUENCIA", out var frecuencia);
            if (string.IsNullOrWhiteSpace(frecuencia))
            {
                errors.Add("FRECUENCIA es obligatoria");
            }
            else if (!ValidFrecuencias.Contains(frecuencia.Trim()))
            {
                errors.Add($"FRECUENCIA '{frecuencia}' no es válida — " +
                           $"valores permitidos: {string.Join(", ", ValidFrecuencias)}");
            }

            // ASEGURADORA — required
            if (!rowData.TryGetValue("ASEGURADORA", out var aseguradora) || string.IsNullOrWhiteSpace(aseguradora))
                errors.Add("ASEGURADORA es obligatoria");

            return errors;
        }

        /// <summary>
        /// Obtiene el valor de celda buscando primero por nombre de columna en el mapa de headers.
        /// Si columnIndex es -1 y el header no se encuentra, retorna string vacío.
        /// Si columnIndex > 0 y el header no se encuentra, usa la posición como fallback.
        /// </summary>
        private string GetCell(IXLRow row, Dictionary<string, int> columnMap, string headerName, int fallbackIndex, bool optional = false)
        {
            if (columnMap.TryGetValue(headerName.ToUpperInvariant(), out var col))
                return GetCellValueSafe(row, col, headerName, optional);

            // No se encontró el header en el mapa
            if (fallbackIndex > 0)
                return GetCellValueSafe(row, fallbackIndex, headerName, optional);

            return string.Empty; // Columna no existe en este archivo
        }

        private string GetCellValueSafe(IXLRow row, int columnNumber, string columnName, bool optional = false)
        {
            try
            {
                var cell = row.Cell(columnNumber);
                if (cell == null || cell.IsEmpty())
                {
                    return string.Empty; // Siempre retornar vacío si no hay valor
                }

                var value = cell.GetString().Trim();
                return value ?? string.Empty; // Retornar vacío si es null
            }
            catch (Exception ex)
            {
                // En caso de error, retornar vacío en lugar de lanzar excepción
                _logger.LogWarning(ex, "Advertencia fila {RowNumber}, columna '{ColumnName}': {ErrorMessage}", row.RowNumber(), columnName, ex.Message);
                return string.Empty;
            }
        }

        private string TruncateString(string value, int maxLength, int rowNumber, string fieldName)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Length <= maxLength)
                return value;

            var truncated = value.Substring(0, maxLength);
            _logger.LogDebug("Fila {RowNumber}: campo '{FieldName}' truncado de {OriginalLength} a {MaxLength} caracteres",
                rowNumber, fieldName, value.Length, maxLength);
            return truncated;
        }

        private decimal ParseDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            var clean = value.Replace("$", "").Replace(" ", "").Trim();

            // Formato español/costarricense: puntos como separador de miles, coma como decimal
            // Ejemplo: "23.586.334,00" → "23586334.00"
            if (clean.Contains(','))
            {
                // La coma es el separador decimal: eliminar puntos (miles) y sustituir coma por punto
                clean = clean.Replace(".", "").Replace(",", ".");
            }
            // Si solo hay puntos (sin coma), podría ser formato inglés con punto decimal
            // Se deja tal cual para que InvariantCulture lo parsee normalmente

            if (decimal.TryParse(clean, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result))
                return result;

            _logger.LogWarning("Valor inválido para prima '{PrimaValue}', se asignará 0", value);
            return 0;
        }

        private DateTime ParseDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return DateTime.Today; // Fecha actual por defecto

            // Intentar múltiples formatos de fecha
            var formats = new[]
            {
                "dd/MM/yyyy",
                "dd-MM-yyyy",
                "dd/MM/yy",
                "dd-MM-yy",
                "MM/dd/yyyy",
                "yyyy-MM-dd",
                "yyyy/MM/dd",
                "d/M/yyyy",
                "d-M-yyyy"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(value.Trim(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                {
                    // Retornar solo la fecha sin horas ni minutos (fecha normalizada a medianoche)
                    return result.Date;
                }
            }

            // Si no puede parsear, retornar fecha actual en lugar de error
            _logger.LogWarning("Formato de fecha inválido '{DateValue}', se usará la fecha actual", value);
            return DateTime.Today;
        }

        private bool IsValidCurrency(string currency)
        {
            var validCurrencies = new[] { "CRC", "USD", "EUR" };
            return validCurrencies.Contains(currency?.ToUpperInvariant());
        }

        private string NormalizeAndValidateCurrency(string currency, int rowNumber)
        {
            if (string.IsNullOrEmpty(currency))
                return currency;

            var upperCurrency = currency.ToUpperInvariant();

            // Convertir formatos largos a códigos ISO
            if (upperCurrency.Contains("COLON") || upperCurrency == "COL")
            {
                _logger.LogDebug("Fila {RowNumber}: moneda '{Currency}' normalizada a 'CRC'", rowNumber, currency);
                return "CRC";
            }
            
            if (upperCurrency.Contains("DOLAR") || upperCurrency.Contains("DOLLAR") || upperCurrency == "DOL")
            {
                _logger.LogDebug("Fila {RowNumber}: moneda '{Currency}' normalizada a 'USD'", rowNumber, currency);
                return "USD";
            }

            if (upperCurrency.Contains("EURO") || upperCurrency == "EUR")
            {
                _logger.LogDebug("Fila {RowNumber}: moneda '{Currency}' normalizada a 'EUR'", rowNumber, currency);
                return "EUR";
            }

            // Si ya es un código válido, devolverlo
            if (IsValidCurrency(currency))
                return currency.ToUpperInvariant();

            // Si no se puede convertir, devolver CRC por defecto
            _logger.LogWarning("Fila {RowNumber}: moneda '{Currency}' no reconocida, se asignará 'CRC' por defecto", rowNumber, currency);
            return "CRC";
        }

        private string GenerateErrorSummary(List<string> errors)
        {
            if (!errors.Any()) return string.Empty;

            // Agrupar errores por tipo
            var errorTypes = new Dictionary<string, int>();
            
            foreach (var error in errors)
            {
                if (error.Contains("Póliza ya existe"))
                {
                    var key = "Números de póliza duplicados";
                    errorTypes[key] = errorTypes.GetValueOrDefault(key, 0) + 1;
                }
                else if (error.Contains("duplicada en el archivo"))
                {
                    var key = "Pólizas duplicadas en el mismo archivo";
                    errorTypes[key] = errorTypes.GetValueOrDefault(key, 0) + 1;
                }
                else
                {
                    var key = "Otros errores de procesamiento";
                    errorTypes[key] = errorTypes.GetValueOrDefault(key, 0) + 1;
                }
            }

            // Crear resumen limitado a los 6 errores más comunes
            var summary = errorTypes
                .OrderByDescending(x => x.Value)
                .Take(6)
                .Select(x => $"• {x.Key}: {x.Value} registro{(x.Value > 1 ? "s" : "")}")
                .ToList();

            var totalErrorsShown = errorTypes.Take(6).Sum(x => x.Value);
            if (errors.Count > totalErrorsShown)
            {
                summary.Add($"• ... y {errors.Count - totalErrorsShown} errores adicionales");
            }

            return string.Join("\n", summary);
        }

        private string ExtractColumnName(string error)
        {
            // Extraer nombre de columna del error "Columna 'NOMBRE' (columna X) está vacía"
            var start = error.IndexOf("'") + 1;
            var end = error.IndexOf("'", start);
            return end > start ? error.Substring(start, end - start) : "desconocida";
        }
    }
}