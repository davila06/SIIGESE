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

    public class CreatePolizaDto
    {
        public string NumeroPoliza { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public string TipoSeguro { get; set; } = string.Empty;
        public string Producto { get; set; } = string.Empty;
        public string PlanCobertura { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoAsegurado { get; set; }
        public decimal PrimaNeta { get; set; }
        public decimal Impuestos { get; set; }
        public decimal PrimaTotal { get; set; }
        public string FormaPago { get; set; } = string.Empty;
        public string FrecuenciaPago { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Moneda { get; set; } = string.Empty;
        public string Beneficiarios { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public string AgenteCodigo { get; set; } = string.Empty;
        public string AgenteNombre { get; set; } = string.Empty;
        public string SucursalCodigo { get; set; } = string.Empty;
        public string SucursalNombre { get; set; } = string.Empty;
        public string CompaniaCodigo { get; set; } = string.Empty;
        public string CompaniaNombre { get; set; } = string.Empty;
        public string DeducibleMaximo { get; set; } = string.Empty;
        public string CoaseguroMaximo { get; set; } = string.Empty;
        public string GastosMedicoMaximo { get; set; } = string.Empty;
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
    }

    public class FailedRecordDto
    {
        public int RowNumber { get; set; }
        public string Error { get; set; } = string.Empty;
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> OriginalData { get; set; } = new Dictionary<string, string>();
    }
}