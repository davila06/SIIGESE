namespace Application.DTOs
{
    public class CreateClienteDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string NumeroIdentificacion { get; set; } = string.Empty;
        public string TipoIdentificacion { get; set; } = string.Empty;
        public string PrimerNombre { get; set; } = string.Empty;
        public string SegundoNombre { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string SegundoApellido { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Genero { get; set; } = string.Empty;
        public string EstadoCivil { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Provincia { get; set; } = string.Empty;
        public string Canton { get; set; } = string.Empty;
        public string Distrito { get; set; } = string.Empty;
        public string CodigoPostal { get; set; } = string.Empty;
        public string Nacionalidad { get; set; } = string.Empty;
        public string Ocupacion { get; set; } = string.Empty;
        public decimal IngresosMensuales { get; set; }
        public string TipoCliente { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para crear o actualizar una póliza desde el formulario frontend.
    /// Todos los campos deben coincidir exactamente con el JSON que envía el frontend (camelCase → PascalCase).
    /// </summary>
    public class CreatePolizaDto
    {
        public int    PerfilId        { get; set; } = 1;
        public string NumeroPoliza    { get; set; } = string.Empty;
        public string Modalidad       { get; set; } = string.Empty;
        public string NombreAsegurado { get; set; } = string.Empty;
        public string NumeroCedula    { get; set; } = string.Empty;
        public decimal Prima          { get; set; }
        public string Moneda          { get; set; } = "CRC";
        /// <summary>Fecha en formato ISO 8601 (YYYY-MM-DD) como la envía el input[type=date] del frontend.</summary>
        public string FechaVigencia   { get; set; } = string.Empty;
        public string Frecuencia      { get; set; } = string.Empty;
        public string Aseguradora     { get; set; } = string.Empty;
        public string Placa           { get; set; } = string.Empty;
        public string Marca           { get; set; } = string.Empty;
        public string Modelo          { get; set; } = string.Empty;
        public string Año             { get; set; } = string.Empty;
        public string Correo          { get; set; } = string.Empty;
        public string NumeroTelefono  { get; set; } = string.Empty;
        public string? Observaciones  { get; set; }
    }

    public class DataUploadResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ProcessedRecords { get; set; }
        public int SuccessfulRecords { get; set; }
        public int FailedRecordsCount { get; set; }
        public int TotalRecords { get; set; }
        public int ErrorRecords { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<FailedRecordDto> FailedRecords { get; set; } = new List<FailedRecordDto>();
        /// <summary>Ordered column headers exactly as they appeared in the uploaded file.</summary>
        public List<string> FileHeaders { get; set; } = new List<string>();
    }

    public class FailedRecordDto
    {
        public int RowNumber { get; set; }
        public string Error { get; set; } = string.Empty;
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> OriginalData { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Request body to generate a correctable Excel file from the failed records
    /// of a previous upload attempt. The resulting file has the same columns as
    /// the original upload file plus a trailing MOTIVO_ERROR column.
    /// </summary>
    public class ErrorsExcelRequestDto
    {
        /// <summary>Column headers exactly as they appeared in the original uploaded file.</summary>
        public List<string> FileHeaders { get; set; } = new List<string>();
        public List<FailedRecordDto> FailedRecords { get; set; } = new List<FailedRecordDto>();
        /// <summary>Original file name used only for the download file name.</summary>
        public string OriginalFileName { get; set; } = "polizas";
    }
}