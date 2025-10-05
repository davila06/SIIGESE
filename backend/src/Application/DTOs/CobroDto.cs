using Domain.Entities;

namespace Application.DTOs
{
    public class CobroDto
    {
        public int Id { get; set; }
        public string NumeroRecibo { get; set; } = string.Empty;
        public int PolizaId { get; set; }
        public string NumeroPoliza { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteApellido { get; set; } = string.Empty;
        public DateTime FechaVencimiento { get; set; }
        public DateTime? FechaCobro { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal? MontoCobrado { get; set; }
        public string Moneda { get; set; } = "CRC"; // Código de moneda
        public string Estado { get; set; } = string.Empty;
        public string? MetodoPago { get; set; }
        public string? Observaciones { get; set; }
        public int? UsuarioCobroId { get; set; }
        public string? UsuarioCobroNombre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }

    public class CobroRequestDto
    {
        public int PolizaId { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoTotal { get; set; }
        public string Moneda { get; set; } = "CRC"; // Código de moneda por defecto
        public string? Observaciones { get; set; }
    }

    public class RegistrarCobroRequestDto
    {
        public int CobroId { get; set; }
        public DateTime FechaCobro { get; set; }
        public decimal MontoCobrado { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
    }

    public class CobroStatsDto
    {
        public int TotalCobros { get; set; }
        public int CobrosPendientes { get; set; }
        public int CobrosCobrados { get; set; }
        public int CobrosVencidos { get; set; }
        public decimal MontoTotalPendiente { get; set; }
        public decimal MontoTotalCobrado { get; set; }
        public decimal MontoTotalVencido { get; set; }
        public IEnumerable<CobroDto> CobrosProximosVencer { get; set; } = new List<CobroDto>();
    }

    public class ActualizarCobroDto
    {
        public DateTime? FechaVencimiento { get; set; }
        public decimal? MontoTotal { get; set; }
        public string? Observaciones { get; set; }
    }
}