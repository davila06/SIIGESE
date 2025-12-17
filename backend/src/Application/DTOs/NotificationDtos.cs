namespace Application.DTOs
{
    public class NotificationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int OverduePaymentsSent { get; set; }
        public int OverduePaymentsFailed { get; set; }
        public int ExpiringPoliciesSent { get; set; }
        public int ExpiringPoliciesFailed { get; set; }
    }

    public class CobroVencidoDto
    {
        public int CobroId { get; set; }
        public string NumeroPoliza { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public decimal MontoVencido { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int DiasMora { get; set; }
        public string Concepto { get; set; } = string.Empty;
    }

    public class PolizaVencimientoDto
    {
        public int PolizaId { get; set; }
        public string NumeroPoliza { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public DateTime FechaVencimiento { get; set; }
        public int DiasHastaVencimiento { get; set; }
        public string TipoPoliza { get; set; } = string.Empty;
        public decimal MontoAsegurado { get; set; }
        public decimal Prima { get; set; }
    }

    public class NotificationStatisticsDto
    {
        public int OverduePaymentsCount { get; set; }
        public int ExpiringPoliciesCount { get; set; }
        public decimal TotalOverdueAmount { get; set; }
        public decimal TotalInsuredAmount { get; set; }
        public int DaysBeforeExpiration { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
