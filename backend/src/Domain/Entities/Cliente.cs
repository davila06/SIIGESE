using System;

namespace Domain.Entities
{
    public class Cliente : Entity
    {
        public string Codigo { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string NombreComercial { get; set; } = string.Empty;
        public string NIT { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public string Pais { get; set; } = "Colombia";
        public bool EsActivo { get; set; } = true;
        public DateTime? FechaRegistro { get; set; }
        public int PerfilId { get; set; }
    }
}
