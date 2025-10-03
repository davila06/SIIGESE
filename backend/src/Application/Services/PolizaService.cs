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
using ClosedXML.Excel;

namespace Application.Services
{
    public class PolizaService : IPolizaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PolizaService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
            var existingPoliza = await _unitOfWork.Polizas.GetByNumeroPolizaAsync(dto.NumeroPoliza);
            if (existingPoliza != null)
                throw new InvalidOperationException("Ya existe una póliza con este número");

            var poliza = _mapper.Map<Poliza>(dto);
            await _unitOfWork.Polizas.AddAsync(poliza);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PolizaDto>(poliza);
        }

        public async Task<PolizaDto> UpdateAsync(int id, CreatePolizaDto dto)
        {
            var poliza = await _unitOfWork.Polizas.GetByIdAsync(id);
            if (poliza == null)
                throw new KeyNotFoundException("Póliza no encontrada");

            _mapper.Map(dto, poliza);
            poliza.UpdatedAt = DateTime.UtcNow;
            
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

                if (result.TotalRecords == 0)
                {
                    errors.Add("El archivo Excel no contiene datos o solo tiene encabezados.");
                    result.ErrorRecords = 0;
                    result.ProcessedRecords = 0;
                }
                else
                {
                    Console.WriteLine($"Procesando {result.TotalRecords} filas del Excel...");
                    foreach (var row in rows)
                    {
                        Console.WriteLine($"Procesando fila {row.RowNumber()}...");
                        try
                        {
                            // Verificar que la fila tenga al menos las columnas mínimas requeridas
                            if (row.CellsUsed().Count() < 8) // Al menos 8 columnas: POLIZA hasta ASEGURADORA
                            {
                                var error = $"Faltan columnas. Se requieren al menos 8 columnas (POLIZA, MOD, NOMBRE, PRIMA, MONEDA, FECHA, FRECUENCIA, ASEGURADORA)";
                                errors.Add($"Fila {row.RowNumber()}: {error}");
                                
                                // Capturar datos originales para el registro fallido
                                var originalData = new Dictionary<string, string>();
                                for (int i = 1; i <= Math.Max(8, row.CellsUsed().Count()); i++)
                                {
                                    originalData[$"Columna{i}"] = GetCellValueSafe(row, i, $"Col{i}", true);
                                }
                                
                                result.FailedRecords.Add(new FailedRecordDto
                                {
                                    RowNumber = row.RowNumber(),
                                    Error = error,
                                    OriginalData = originalData
                                });
                                
                                result.ErrorRecords++;
                                continue;
                            }

                            // Capturar datos originales de la fila para uso posterior
                            var rowData = new Dictionary<string, string>
                            {
                                ["POLIZA"] = GetCellValueSafe(row, 1, "POLIZA", true),
                                ["MOD"] = GetCellValueSafe(row, 2, "MOD", true),
                                ["NOMBRE"] = GetCellValueSafe(row, 3, "NOMBRE", true),
                                ["PRIMA"] = GetCellValueSafe(row, 4, "PRIMA", true),
                                ["MONEDA"] = GetCellValueSafe(row, 5, "MONEDA", true),
                                ["FECHA"] = GetCellValueSafe(row, 6, "FECHA", true),
                                ["FRECUENCIA"] = GetCellValueSafe(row, 7, "FRECUENCIA", true),
                                ["ASEGURADORA"] = GetCellValueSafe(row, 8, "ASEGURADORA", true),
                                ["PLACA"] = GetCellValueSafe(row, 9, "PLACA", true),
                                ["MARCA"] = GetCellValueSafe(row, 10, "MARCA", true),
                                ["MODELO"] = GetCellValueSafe(row, 11, "MODELO", true)
                            };

                            // Mapear columnas específicas del Excel
                            // POLIZA	MOD	NOMBRE	PRIMA	MONEDA	FECHA	FRECUENCIA	ASEGURADORA	PLACA	MARCA	MODELO
                            var poliza = new Poliza
                            {
                                NumeroPoliza = TruncateString(GetCellValueSafe(row, 1, "POLIZA"), 50, row.RowNumber(), "POLIZA"),
                                Modalidad = TruncateString(GetCellValueSafe(row, 2, "MOD", true), 50, row.RowNumber(), "MOD"), // Hacer MOD opcional temporalmente
                                NombreAsegurado = TruncateString(GetCellValueSafe(row, 3, "NOMBRE"), 200, row.RowNumber(), "NOMBRE"),
                                Prima = ParseDecimal(GetCellValueSafe(row, 4, "PRIMA")),
                                Moneda = GetCellValueSafe(row, 5, "MONEDA").ToUpperInvariant(), // No truncar moneda, será normalizada después
                                FechaVigencia = ParseDate(GetCellValueSafe(row, 6, "FECHA")),
                                Frecuencia = TruncateString(GetCellValueSafe(row, 7, "FRECUENCIA"), 50, row.RowNumber(), "FRECUENCIA"),
                                Aseguradora = TruncateString(GetCellValueSafe(row, 8, "ASEGURADORA"), 100, row.RowNumber(), "ASEGURADORA"),
                                Placa = TruncateString(GetCellValueSafe(row, 9, "PLACA", true), 8, row.RowNumber(), "PLACA"), // Máximo 8 caracteres para placa
                                Marca = TruncateString(GetCellValueSafe(row, 10, "MARCA", true), 50, row.RowNumber(), "MARCA"),
                                Modelo = TruncateString(GetCellValueSafe(row, 11, "MODELO", true), 50, row.RowNumber(), "MODELO"),
                                PerfilId = perfilId,
                                CreatedBy = userId.ToString()
                            };

                        // Corregir MOD vacío automáticamente
                        if (string.IsNullOrEmpty(poliza.Modalidad))
                        {
                            poliza.Modalidad = "GENERAL"; // Valor por defecto
                            Console.WriteLine($"Fila {row.RowNumber()}: MOD vacío, asignado 'GENERAL' por defecto");
                        }

                        // Validaciones básicas
                        if (string.IsNullOrEmpty(poliza.NumeroPoliza) || 
                            string.IsNullOrEmpty(poliza.NombreAsegurado) || 
                            string.IsNullOrEmpty(poliza.Aseguradora))
                        {
                            var error = "Campos obligatorios vacíos (POLIZA, NOMBRE, ASEGURADORA)";
                            errors.Add($"Fila {row.RowNumber()}: {error}");
                            
                            result.FailedRecords.Add(new FailedRecordDto
                            {
                                RowNumber = row.RowNumber(),
                                Error = error,
                                OriginalData = rowData
                            });
                            
                            result.ErrorRecords++;
                            continue;
                        }

                        // Validar y corregir moneda automáticamente
                        poliza.Moneda = NormalizeAndValidateCurrency(poliza.Moneda, row.RowNumber());

                        Console.WriteLine($"Fila {row.RowNumber()}: Datos procesados, verificando duplicados...");

                        // Verificar duplicados
                        var existingPoliza = await _unitOfWork.Polizas.GetByNumeroPolizaAsync(poliza.NumeroPoliza);
                        if (existingPoliza != null)
                        {
                            var error = $"Póliza ya existe (Número: {poliza.NumeroPoliza})";
                            errors.Add($"Fila {row.RowNumber()}: {error}");
                            
                            result.FailedRecords.Add(new FailedRecordDto
                            {
                                RowNumber = row.RowNumber(),
                                Error = error,
                                OriginalData = rowData
                            });
                            
                            result.ErrorRecords++;
                            continue;
                        }

                        // Verificar duplicados en el lote actual
                        var duplicateInBatch = polizas.FirstOrDefault(p => p.NumeroPoliza == poliza.NumeroPoliza);
                        if (duplicateInBatch != null)
                        {
                            var error = $"Póliza duplicada en el archivo (Número: {poliza.NumeroPoliza})";
                            errors.Add($"Fila {row.RowNumber()}: {error}");
                            
                            result.FailedRecords.Add(new FailedRecordDto
                            {
                                RowNumber = row.RowNumber(),
                                Error = error,
                                OriginalData = rowData
                            });
                            
                            result.ErrorRecords++;
                            continue;
                        }

                        // Si llegamos aquí, el registro es válido
                        polizas.Add(poliza);
                        result.ProcessedRecords++;
                        Console.WriteLine($"Fila {row.RowNumber()}: ¡Registro válido agregado! Total válidos: {result.ProcessedRecords}");
                    }
                    catch (Exception ex)
                    {
                        var error = ex.Message;
                        errors.Add($"Fila {row.RowNumber()}: {error}");
                        
                        // Intentar capturar datos originales incluso en caso de excepción
                        var originalData = new Dictionary<string, string>();
                        try
                        {
                            for (int i = 1; i <= 11; i++)
                            {
                                var columnNames = new[] { "", "POLIZA", "MOD", "NOMBRE", "PRIMA", "MONEDA", "FECHA", "FRECUENCIA", "ASEGURADORA", "PLACA", "MARCA", "MODELO" };
                                var columnName = i < columnNames.Length ? columnNames[i] : $"Col{i}";
                                originalData[columnName] = GetCellValueSafe(row, i, columnName, true);
                            }
                        }
                        catch
                        {
                            originalData["Error"] = "No se pudieron extraer los datos originales";
                        }
                        
                        result.FailedRecords.Add(new FailedRecordDto
                        {
                            RowNumber = row.RowNumber(),
                            Error = error,
                            OriginalData = originalData
                        });
                        
                        result.ErrorRecords++;
                        Console.WriteLine($"Error en fila {row.RowNumber()}: {ex.Message}");
                        Console.WriteLine($"StackTrace: {ex.StackTrace}");
                    }
                }

                Console.WriteLine($"Resumen procesamiento: {result.ProcessedRecords} válidos, {result.ErrorRecords} errores de {result.TotalRecords} total");

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

        private string GetCellValueSafe(IXLRow row, int columnNumber, string columnName, bool optional = false)
        {
            try
            {
                var cell = row.Cell(columnNumber);
                if (cell == null || cell.IsEmpty())
                {
                    if (optional)
                        return string.Empty;
                    else
                        throw new InvalidOperationException($"Fila {row.RowNumber()}: Columna '{columnName}' (columna {columnNumber}) está vacía");
                }

                var value = cell.GetString().Trim();
                if (string.IsNullOrEmpty(value) && !optional)
                {
                    throw new InvalidOperationException($"Fila {row.RowNumber()}: Columna '{columnName}' (columna {columnNumber}) está vacía");
                }

                return value;
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                throw new InvalidOperationException($"Fila {row.RowNumber()}: Error leyendo columna '{columnName}' (columna {columnNumber}): {ex.Message}");
            }
        }

        private string TruncateString(string value, int maxLength, int rowNumber, string fieldName)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Length <= maxLength)
                return value;

            var truncated = value.Substring(0, maxLength);
            Console.WriteLine($"Fila {rowNumber}: Campo '{fieldName}' truncado de '{value}' a '{truncated}' (máximo {maxLength} caracteres)");
            return truncated;
        }

        private decimal ParseDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            // Remover caracteres no numéricos excepto punto y coma
            var cleanValue = value.Replace("$", "").Replace(",", "").Trim();
            
            if (decimal.TryParse(cleanValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result))
                return result;
            
            throw new FormatException($"Valor inválido para prima: '{value}'");
        }

        private DateTime ParseDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return DateTime.Today;

            // Intentar múltiples formatos de fecha
            var formats = new[]
            {
                "dd/MM/yyyy",
                "MM/dd/yyyy",
                "yyyy-MM-dd",
                "dd-MM-yyyy",
                "yyyy/MM/dd"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(value.Trim(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    return result;
            }

            throw new FormatException($"Formato de fecha inválido: '{value}'");
        }

        private bool IsValidCurrency(string currency)
        {
            var validCurrencies = new[] { "COP", "USD", "EUR" };
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
                Console.WriteLine($"Fila {rowNumber}: Moneda '{currency}' convertida automáticamente a 'COP'");
                return "COP";
            }
            
            if (upperCurrency.Contains("DOLAR") || upperCurrency.Contains("DOLLAR") || upperCurrency == "DOL")
            {
                Console.WriteLine($"Fila {rowNumber}: Moneda '{currency}' convertida automáticamente a 'USD'");
                return "USD";
            }

            if (upperCurrency.Contains("EURO") || upperCurrency == "EUR")
            {
                Console.WriteLine($"Fila {rowNumber}: Moneda '{currency}' convertida automáticamente a 'EUR'");
                return "EUR";
            }

            // Si ya es un código válido, devolverlo
            if (IsValidCurrency(currency))
                return currency.ToUpperInvariant();

            // Si no se puede convertir, devolver COP por defecto
            Console.WriteLine($"Fila {rowNumber}: Moneda '{currency}' no reconocida, asignando 'COP' por defecto");
            return "COP";
        }

        private string GenerateErrorSummary(List<string> errors)
        {
            if (!errors.Any()) return string.Empty;

            // Agrupar errores por tipo
            var errorTypes = new Dictionary<string, int>();
            
            foreach (var error in errors)
            {
                if (error.Contains("Columna") && error.Contains("está vacía"))
                {
                    var columnName = ExtractColumnName(error);
                    var key = $"Campo '{columnName}' vacío";
                    errorTypes[key] = errorTypes.GetValueOrDefault(key, 0) + 1;
                }
                else if (error.Contains("Moneda inválida"))
                {
                    var key = "Moneda inválida (use: COP, USD, EUR)";
                    errorTypes[key] = errorTypes.GetValueOrDefault(key, 0) + 1;
                }
                else if (error.Contains("Faltan columnas"))
                {
                    var key = "Faltan columnas requeridas en el Excel";
                    errorTypes[key] = errorTypes.GetValueOrDefault(key, 0) + 1;
                }
                else if (error.Contains("Campos obligatorios vacíos"))
                {
                    var key = "Campos obligatorios sin datos";
                    errorTypes[key] = errorTypes.GetValueOrDefault(key, 0) + 1;
                }
                else if (error.Contains("Póliza ya existe"))
                {
                    var key = "Números de póliza duplicados";
                    errorTypes[key] = errorTypes.GetValueOrDefault(key, 0) + 1;
                }
                else if (error.Contains("duplicada en el archivo"))
                {
                    var key = "Pólizas duplicadas en el mismo archivo";
                    errorTypes[key] = errorTypes.GetValueOrDefault(key, 0) + 1;
                }
                else if (error.Contains("Formato de fecha inválido"))
                {
                    var key = "Fechas con formato incorrecto";
                    errorTypes[key] = errorTypes.GetValueOrDefault(key, 0) + 1;
                }
                else if (error.Contains("Valor inválido para prima"))
                {
                    var key = "Valores de prima inválidos";
                    errorTypes[key] = errorTypes.GetValueOrDefault(key, 0) + 1;
                }
                else
                {
                    var key = "Otros errores de formato";
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