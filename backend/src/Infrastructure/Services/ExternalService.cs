using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;

namespace Infrastructure.Services
{
    public interface IExcelService
    {
        Task<bool> ValidateExcelFileAsync(IFormFile file);
        Task<IEnumerable<T>> ReadExcelAsync<T>(IFormFile file, Func<IXLRow, T> mapper) where T : class;
        Task<byte[]> GenerateExcelAsync<T>(IEnumerable<T> data, string[] headers, Func<T, object[]> mapper);
    }

    public class ExcelService : IExcelService
    {
        private readonly ILogger<ExcelService> _logger;
        private readonly string[] _allowedExtensions = { ".xlsx", ".xls" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public ExcelService(ILogger<ExcelService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> ValidateExcelFileAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Archivo no proporcionado o vacío");
                    return false;
                }

                if (file.Length > MaxFileSize)
                {
                    _logger.LogWarning("Archivo excede el tamaño máximo permitido: {Size}MB", file.Length / 1024 / 1024);
                    return false;
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!Array.Exists(_allowedExtensions, ext => ext == extension))
                {
                    _logger.LogWarning("Extensión de archivo no permitida: {Extension}", extension);
                    return false;
                }

                // Validar que realmente sea un archivo Excel
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                
                if (workbook.Worksheets.Count == 0)
                {
                    _logger.LogWarning("El archivo Excel no contiene hojas de trabajo");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando archivo Excel");
                return false;
            }
        }

        public async Task<IEnumerable<T>> ReadExcelAsync<T>(IFormFile file, Func<IXLRow, T> mapper) where T : class
        {
            var results = new List<T>();

            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);

                var rows = worksheet.RowsUsed().Skip(1); // Omitir header

                await Task.Run(() =>
                {
                    foreach (var row in rows)
                    {
                        try
                        {
                            var item = mapper(row);
                            if (item != null)
                                results.Add(item);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error procesando fila {RowNumber}", row.RowNumber());
                        }
                    }
                });

                _logger.LogInformation("Procesadas {Count} filas del archivo Excel", results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leyendo archivo Excel");
                throw new InvalidOperationException("Error procesando archivo Excel", ex);
            }
        }

        public async Task<byte[]> GenerateExcelAsync<T>(IEnumerable<T> data, string[] headers, Func<T, object[]> mapper)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Data");

                // Agregar headers
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                }

                // Agregar datos
                var rowIndex = 2;
                await Task.Run(() =>
                {
                    foreach (var item in data)
                    {
                        var values = mapper(item);
                        for (int i = 0; i < values.Length; i++)
                        {
                            worksheet.Cell(rowIndex, i + 1).Value = values[i]?.ToString() ?? "";
                        }
                        rowIndex++;
                    }
                });

                // Auto-ajustar columnas
                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando archivo Excel");
                throw new InvalidOperationException("Error generando archivo Excel", ex);
            }
        }
    }


}