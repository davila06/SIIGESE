using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    public class LoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; } = null!;
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<RoleDto> Roles { get; set; } = new();
    }

    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CreateUserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public List<int> RoleIds { get; set; } = new();
    }

    public class UpdateUserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<int> RoleIds { get; set; } = new();
    }

    public class ClienteDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string NombreComercial { get; set; } = string.Empty;
        public string NIT { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public bool EsActivo { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public int PerfilId { get; set; }
    }

    public class CreateClienteDto
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
        public int PerfilId { get; set; }
    }

    public class DataUploadResultDto
    {
        public int TotalRecords { get; set; }
        public int ProcessedRecords { get; set; }
        public int ErrorRecords { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<FailedRecordDto> FailedRecords { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool Success => ProcessedRecords > 0 || ErrorRecords == 0;
    }

    public class FailedRecordDto
    {
        public int RowNumber { get; set; }
        public string Error { get; set; } = string.Empty;
        public Dictionary<string, string> OriginalData { get; set; } = new();
    }

    public class PolizaDto
    {
        public int Id { get; set; }
        public string NumeroPoliza { get; set; } = string.Empty;
        public string Modalidad { get; set; } = string.Empty;
        public string NombreAsegurado { get; set; } = string.Empty;
        public decimal Prima { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public DateTime FechaVigencia { get; set; }
        public string Frecuencia { get; set; } = string.Empty;
        public string Aseguradora { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int PerfilId { get; set; }
        public bool EsActivo { get; set; }
    }

    public class CreatePolizaDto
    {
        public string NumeroPoliza { get; set; } = string.Empty;
        public string Modalidad { get; set; } = string.Empty;
        public string NombreAsegurado { get; set; } = string.Empty;
        public decimal Prima { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public DateTime FechaVigencia { get; set; }
        public string Frecuencia { get; set; } = string.Empty;
        public string Aseguradora { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int PerfilId { get; set; }
    }
}