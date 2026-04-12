using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using ClosedXML.Excel;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Mail;

namespace Application.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ClienteService> _logger;

        private static readonly string[] HeaderAliasesRazonSocial =
            { "RAZONSOCIAL", "RAZON SOCIAL", "NOMBRE", "NOMBRE CLIENTE", "CLIENTE" };

        private static readonly string[] HeaderAliasesNIT =
            { "NIT", "IDENTIFICACION", "NUMEROIDENTIFICACION", "NUMERO IDENTIFICACION" };

        private static readonly string[] HeaderAliasesCodigo =
            { "CODIGO", "CODIGOCLIENTE", "CODIGO CLIENTE" };

        private static readonly string[] HeaderAliasesEmail =
            { "EMAIL", "CORREO", "CORREOELECTRONICO", "CORREO ELECTRONICO" };

        private static readonly string[] HeaderAliasesTelefono =
            { "TELEFONO", "CELULAR", "MOVIL", "NUMEROTELEFONO", "NUMERO TELEFONO" };

        private static readonly string[] HeaderAliasesDireccion =
            { "DIRECCION", "DOMICILIO" };

        private static readonly string[] HeaderAliasesNombreComercial =
            { "NOMBRECOMERCIAL", "NOMBRE COMERCIAL" };

        private static readonly string[] HeaderAliasesCiudad =
            { "CIUDAD", "CANTON" };

        private static readonly string[] HeaderAliasesDepartamento =
            { "DEPARTAMENTO", "PROVINCIA" };

        private static readonly string[] HeaderAliasesPais =
            { "PAIS", "PAIS/REGION", "NACIONALIDAD" };

        public ClienteService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ClienteService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ClienteDto>> GetAllAsync()
        {
            var clientes = await _unitOfWork.Clientes.GetAllAsync();
            return _mapper.Map<IEnumerable<ClienteDto>>(clientes);
        }

        public async Task<IEnumerable<ClienteDto>> GetByPerfilIdAsync(int perfilId)
        {
            var clientes = await _unitOfWork.Clientes.GetByPerfilIdAsync(perfilId);
            return _mapper.Map<IEnumerable<ClienteDto>>(clientes);
        }

        public async Task<ClienteDto?> GetByIdAsync(int id)
        {
            var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
            return cliente == null ? null : _mapper.Map<ClienteDto>(cliente);
        }

        public async Task<ClienteDto> CreateAsync(CreateClienteDto dto)
        {
            var cliente = new Cliente();
            ApplyDto(dto, cliente);
            cliente.FechaRegistro = DateTime.UtcNow;
            await _unitOfWork.Clientes.AddAsync(cliente);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ClienteDto>(cliente);
        }

        public async Task<ClienteDto> UpdateAsync(int id, CreateClienteDto dto)
        {
            var cliente = await _unitOfWork.Clientes.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Cliente con ID {id} no encontrado.");
            ApplyDto(dto, cliente);
            await _unitOfWork.Clientes.UpdateAsync(cliente);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ClienteDto>(cliente);
        }

        public async Task DeleteAsync(int id)
        {
            var cliente = await _unitOfWork.Clientes.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Cliente con ID {id} no encontrado.");
            await _unitOfWork.Clientes.DeleteAsync(cliente);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<DataUploadResultDto> ProcesarExcelAsync(int perfilId, IFormFile file, int userId)
        {
            var result = new DataUploadResultDto();
            var errors = new List<string>();
            var clientesNuevos = new List<Cliente>();
            var clientesActualizar = new List<Cliente>();
            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
                    throw new InvalidOperationException("El archivo Excel no contiene hojas de trabajo.");

                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1).ToList();
                result.TotalRecords = rows.Count;

                var headerMap = BuildHeaderMap(worksheet.Row(1));
                result.FileHeaders = headerMap
                    .OrderBy(kvp => kvp.Value)
                    .Select(kvp => kvp.Key)
                    .ToList();

                var colRazonSocial = ResolveColumnIndex(headerMap, HeaderAliasesRazonSocial);
                var colNit = ResolveColumnIndex(headerMap, HeaderAliasesNIT);
                var colCodigo = ResolveColumnIndex(headerMap, HeaderAliasesCodigo);
                var colEmail = ResolveColumnIndex(headerMap, HeaderAliasesEmail);
                var colTelefono = ResolveColumnIndex(headerMap, HeaderAliasesTelefono);
                var colDireccion = ResolveColumnIndex(headerMap, HeaderAliasesDireccion);
                var colNombreComercial = ResolveColumnIndex(headerMap, HeaderAliasesNombreComercial);
                var colCiudad = ResolveColumnIndex(headerMap, HeaderAliasesCiudad);
                var colDepartamento = ResolveColumnIndex(headerMap, HeaderAliasesDepartamento);
                var colPais = ResolveColumnIndex(headerMap, HeaderAliasesPais);

                var missingColumns = new List<string>();
                if (colRazonSocial == null) missingColumns.Add("RazonSocial/Nombre");
                if (colNit == null) missingColumns.Add("NIT");
                if (colEmail == null) missingColumns.Add("Email");
                if (colTelefono == null) missingColumns.Add("Telefono");
                if (colDireccion == null) missingColumns.Add("Direccion");

                if (missingColumns.Any())
                {
                    var missingMsg = $"Columnas requeridas faltantes: {string.Join(", ", missingColumns)}";
                    errors.Add(missingMsg);
                    result.Message = missingMsg;
                    result.Status = "Failed";
                    result.Success = false;
                    result.Errors = errors;

                    dataRecord.Status = "Failed";
                    dataRecord.TotalRecords = result.TotalRecords;
                    dataRecord.ProcessedRecords = 0;
                    dataRecord.ErrorRecords = 0;
                    dataRecord.ErrorDetails = missingMsg;
                    await _unitOfWork.DataRecords.UpdateAsync(dataRecord);
                    await _unitOfWork.SaveChangesAsync();
                    return result;
                }

                foreach (var row in rows)
                {
                    var rowData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["CODIGO"] = GetCellValue(row, colCodigo),
                        ["RAZONSOCIAL"] = GetCellValue(row, colRazonSocial),
                        ["NIT"] = GetCellValue(row, colNit),
                        ["EMAIL"] = GetCellValue(row, colEmail),
                        ["TELEFONO"] = GetCellValue(row, colTelefono),
                        ["DIRECCION"] = GetCellValue(row, colDireccion),
                        ["NOMBRECOMERCIAL"] = GetCellValue(row, colNombreComercial),
                        ["CIUDAD"] = GetCellValue(row, colCiudad),
                        ["DEPARTAMENTO"] = GetCellValue(row, colDepartamento),
                        ["PAIS"] = GetCellValue(row, colPais)
                    };

                    try
                    {
                        var validationErrors = ValidateClienteRow(rowData);
                        if (validationErrors.Any())
                        {
                            var errorMsg = string.Join("; ", validationErrors);
                            errors.Add($"Fila {row.RowNumber()}: {errorMsg}");
                            result.FailedRecords.Add(new FailedRecordDto
                            {
                                RowNumber = row.RowNumber(),
                                Error = errorMsg,
                                OriginalData = rowData
                            });
                            result.ErrorRecords++;
                            continue;
                        }

                        var normalizedNit = rowData["NIT"].Trim();
                        var normalizedCode = !string.IsNullOrWhiteSpace(rowData["CODIGO"])
                            ? rowData["CODIGO"].Trim()
                            : BuildCodigoFromNit(normalizedNit);

                        var dedupeKey = $"NIT:{normalizedNit.ToUpperInvariant()}|COD:{normalizedCode.ToUpperInvariant()}";
                        if (!seenKeys.Add(dedupeKey))
                        {
                            var duplicateMsg = $"Registro duplicado en el archivo para NIT '{normalizedNit}' y Codigo '{normalizedCode}'";
                            errors.Add($"Fila {row.RowNumber()}: {duplicateMsg}");
                            result.FailedRecords.Add(new FailedRecordDto
                            {
                                RowNumber = row.RowNumber(),
                                Error = duplicateMsg,
                                OriginalData = rowData
                            });
                            result.ErrorRecords++;
                            continue;
                        }

                        var existing = await _unitOfWork.Clientes.GetByNITAsync(normalizedNit);
                        if (existing == null)
                        {
                            existing = await _unitOfWork.Clientes.GetByCodigoAsync(normalizedCode);
                        }

                        if (existing != null)
                        {
                            ApplyRowDataToCliente(existing, rowData, perfilId, userId, isUpdate: true);
                            clientesActualizar.Add(existing);
                        }
                        else
                        {
                            var nuevo = new Cliente();
                            ApplyRowDataToCliente(nuevo, rowData, perfilId, userId, isUpdate: false);
                            clientesNuevos.Add(nuevo);
                        }

                        result.ProcessedRecords++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Fila {row.RowNumber()}: {ex.Message}");
                        result.FailedRecords.Add(new FailedRecordDto
                        {
                            RowNumber = row.RowNumber(),
                            Error = ex.Message,
                            OriginalData = rowData
                        });
                        result.ErrorRecords++;
                        _logger.LogWarning(ex, "Error procesando fila {RowNumber} de clientes: {ErrorMessage}", row.RowNumber(), ex.Message);
                    }
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    if (clientesNuevos.Any())
                    {
                        await _unitOfWork.Clientes.AddRangeAsync(clientesNuevos);
                    }

                    foreach (var cliente in clientesActualizar)
                    {
                        await _unitOfWork.Clientes.UpdateAsync(cliente);
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }

                dataRecord.TotalRecords = result.TotalRecords;
                dataRecord.ProcessedRecords = result.ProcessedRecords;
                dataRecord.ErrorRecords = result.ErrorRecords;

                if (result.ProcessedRecords > 0 && result.ErrorRecords == 0)
                {
                    dataRecord.Status = "Completed";
                    result.Message = $"Archivo procesado exitosamente. {result.ProcessedRecords} clientes importados sin errores.";
                }
                else if (result.ProcessedRecords > 0)
                {
                    dataRecord.Status = "Completed with errors";
                    result.Message = $"Archivo procesado parcialmente. {result.ProcessedRecords} clientes importados y {result.ErrorRecords} registros con errores.";
                }
                else if (result.TotalRecords == 0)
                {
                    dataRecord.Status = "No data";
                    result.Message = "El archivo no contiene filas de datos para procesar.";
                }
                else
                {
                    dataRecord.Status = "Failed";
                    result.Message = $"No se pudo importar ningun cliente. {result.ErrorRecords} errores encontrados.";
                }

                dataRecord.ErrorDetails = string.Join(";", errors.Take(50));
                await _unitOfWork.DataRecords.UpdateAsync(dataRecord);
                await _unitOfWork.SaveChangesAsync();

                result.Status = dataRecord.Status;
                result.Errors = errors;
                result.Success = result.ProcessedRecords > 0;
                result.SuccessfulRecords = result.ProcessedRecords;
                result.FailedRecordsCount = result.ErrorRecords;

                return result;
            }
            catch (Exception ex)
            {
                dataRecord.Status = "Failed";
                dataRecord.ErrorDetails = ex.Message;
                await _unitOfWork.DataRecords.UpdateAsync(dataRecord);
                await _unitOfWork.SaveChangesAsync();

                throw new InvalidOperationException($"Error procesando archivo Excel de clientes: {ex.Message}", ex);
            }
        }

        private static Dictionary<string, int> BuildHeaderMap(IXLRow headerRow)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in headerRow.CellsUsed())
            {
                var normalized = NormalizeHeader(cell.GetString());
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    map[normalized] = cell.Address.ColumnNumber;
                }
            }
            return map;
        }

        private static int? ResolveColumnIndex(Dictionary<string, int> headerMap, params string[] aliases)
        {
            foreach (var alias in aliases)
            {
                var normalized = NormalizeHeader(alias);
                if (headerMap.TryGetValue(normalized, out var idx))
                    return idx;
            }
            return null;
        }

        private static string NormalizeHeader(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            return value
                .Trim()
                .ToUpperInvariant()
                .Replace("Á", "A")
                .Replace("É", "E")
                .Replace("Í", "I")
                .Replace("Ó", "O")
                .Replace("Ú", "U")
                .Replace("Ñ", "N")
                .Replace("_", " ")
                .Replace("-", " ")
                .Replace("  ", " ");
        }

        private static string GetCellValue(IXLRow row, int? columnIndex)
        {
            if (columnIndex == null) return string.Empty;
            try
            {
                return row.Cell(columnIndex.Value).GetString().Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static List<string> ValidateClienteRow(Dictionary<string, string> rowData)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(rowData["RAZONSOCIAL"]))
                errors.Add("RazonSocial/Nombre es obligatorio");

            if (string.IsNullOrWhiteSpace(rowData["NIT"]))
                errors.Add("NIT es obligatorio");

            if (string.IsNullOrWhiteSpace(rowData["EMAIL"]))
            {
                errors.Add("Email es obligatorio");
            }
            else if (!IsValidEmail(rowData["EMAIL"]))
            {
                errors.Add($"Email '{rowData["EMAIL"]}' no tiene un formato valido");
            }

            if (string.IsNullOrWhiteSpace(rowData["TELEFONO"]))
                errors.Add("Telefono es obligatorio");

            if (string.IsNullOrWhiteSpace(rowData["DIRECCION"]))
                errors.Add("Direccion es obligatoria");

            return errors;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                _ = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string BuildCodigoFromNit(string nit)
        {
            var safeNit = new string((nit ?? string.Empty).Where(char.IsLetterOrDigit).ToArray());
            if (string.IsNullOrWhiteSpace(safeNit))
                return $"CLI-{Guid.NewGuid():N}".Substring(0, 20);

            var candidate = $"CLI-{safeNit}";
            return candidate.Length <= 20 ? candidate : candidate.Substring(0, 20);
        }

        private static string Truncate(string value, int max)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var trimmed = value.Trim();
            return trimmed.Length <= max ? trimmed : trimmed.Substring(0, max);
        }

        private static void ApplyRowDataToCliente(Cliente cliente, Dictionary<string, string> rowData, int perfilId, int userId, bool isUpdate)
        {
            var nit = Truncate(rowData["NIT"], 20);
            var codigo = !string.IsNullOrWhiteSpace(rowData["CODIGO"])
                ? Truncate(rowData["CODIGO"], 20)
                : BuildCodigoFromNit(nit);

            cliente.Codigo = codigo;
            cliente.RazonSocial = Truncate(rowData["RAZONSOCIAL"], 200);
            cliente.NombreComercial = Truncate(rowData["NOMBRECOMERCIAL"], 200);
            cliente.NIT = nit;
            cliente.Telefono = Truncate(rowData["TELEFONO"], 20);
            cliente.Email = Truncate(rowData["EMAIL"], 100);
            cliente.Direccion = Truncate(rowData["DIRECCION"], 300);
            cliente.Ciudad = Truncate(rowData["CIUDAD"], 100);
            cliente.Departamento = Truncate(rowData["DEPARTAMENTO"], 100);
            cliente.Pais = string.IsNullOrWhiteSpace(rowData["PAIS"]) ? "Colombia" : Truncate(rowData["PAIS"], 100);
            cliente.PerfilId = perfilId;
            cliente.EsActivo = true;

            if (isUpdate)
            {
                cliente.UpdatedAt = DateTime.UtcNow;
                cliente.UpdatedBy = userId.ToString(CultureInfo.InvariantCulture);
                if (!cliente.FechaRegistro.HasValue)
                    cliente.FechaRegistro = DateTime.UtcNow;
            }
            else
            {
                cliente.CreatedAt = DateTime.UtcNow;
                cliente.CreatedBy = userId.ToString(CultureInfo.InvariantCulture);
                cliente.FechaRegistro = DateTime.UtcNow;
            }
        }

        private static void ApplyDto(CreateClienteDto dto, Cliente cliente)
        {
            var partes = new[]
            {
                dto.PrimerNombre?.Trim(),
                dto.SegundoNombre?.Trim(),
                dto.PrimerApellido?.Trim(),
                dto.SegundoApellido?.Trim()
            }.Where(p => !string.IsNullOrWhiteSpace(p));

            cliente.RazonSocial = string.Join(" ", partes);
            cliente.Codigo = dto.Codigo ?? string.Empty;
            cliente.NIT = dto.NumeroIdentificacion ?? string.Empty;
            cliente.Telefono = dto.Telefono ?? string.Empty;
            cliente.Email = dto.Email ?? string.Empty;
            cliente.Direccion = dto.Direccion ?? string.Empty;
            cliente.Ciudad = dto.Canton ?? string.Empty;
            cliente.Departamento = dto.Provincia ?? string.Empty;
            cliente.EsActivo = true;
        }
    }
}
