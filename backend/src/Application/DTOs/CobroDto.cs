using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    public class CobroDto
    {
        public int Id { get; set; }
        public string NumeroRecibo { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public decimal MontoCobrado { get; set; }
        public DateTime FechaCobro { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public string Moneda { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public int PolizaId { get; set; }
        public string NumeroPoliza { get; set; } = string.Empty;
        public string ClienteNombreCompleto { get; set; } = string.Empty;
        public string? CorreoElectronico { get; set; }
        public int UsuarioCobroId { get; set; }
        public string UsuarioCobroNombre { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class GenerarCobrosResultDto
    {
        public int CobrosGenerados { get; set; }
        public int PolizasProcesadas { get; set; }
        public int PolizasSaltadas { get; set; }
        public List<string> Errores { get; set; } = new List<string>();
        public List<CobroGeneradoDto> CobrosCreados { get; set; } = new List<CobroGeneradoDto>();
    }

    public class CobroGeneradoDto
    {
        public string NumeroRecibo { get; set; } = string.Empty;
        public string NumeroPoliza { get; set; } = string.Empty;
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoTotal { get; set; }
        public string Moneda { get; set; } = string.Empty;
    }
}
