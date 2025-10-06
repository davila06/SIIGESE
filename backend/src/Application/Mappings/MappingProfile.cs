using AutoMapper;
using Application.DTOs;
using Domain.Entities;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => 
                    src.UserRoles.Select(ur => ur.Role)));

            CreateMap<Role, RoleDto>();

            // Cliente mappings
            CreateMap<Cliente, ClienteDto>();
            CreateMap<CreateClienteDto, Cliente>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.EsActivo, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.FechaRegistro, opt => opt.MapFrom(src => DateTime.UtcNow));

            // DataRecord mappings
            CreateMap<DataRecord, DataUploadResultDto>()
                .ForMember(dest => dest.Errors, opt => opt.MapFrom(src => 
                    string.IsNullOrEmpty(src.ErrorDetails) ? new List<string>() : 
                    src.ErrorDetails.Split(';', StringSplitOptions.None).ToList()));

            // Poliza mappings
            CreateMap<Poliza, PolizaDto>();
            CreateMap<CreatePolizaDto, Poliza>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.EsActivo, opt => opt.MapFrom(src => true));

            // Cobro mappings
            CreateMap<Cobro, CobroDto>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => (int)src.Estado))
                .ForMember(dest => dest.MetodoPago, opt => opt.MapFrom(src => 
                    src.MetodoPago.HasValue ? (int)src.MetodoPago.Value : (int?)null))
                .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.FechaActualizacion, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.ClienteNombre, opt => opt.MapFrom(src => FixEncoding(src.ClienteNombre)))
                .ForMember(dest => dest.ClienteApellido, opt => opt.MapFrom(src => FixEncoding(src.ClienteApellido)));

            // Reclamo mappings
            CreateMap<Reclamo, ReclamoDto>()
                .ForMember(dest => dest.TipoReclamo, opt => opt.MapFrom(src => src.TipoReclamo.ToString()))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
                .ForMember(dest => dest.Prioridad, opt => opt.MapFrom(src => src.Prioridad.ToString()));

            CreateMap<CreateReclamoDto, Reclamo>()
                .ForMember(dest => dest.TipoReclamo, opt => opt.MapFrom(src => (TipoReclamo)src.TipoReclamo))
                .ForMember(dest => dest.Prioridad, opt => opt.MapFrom(src => (PrioridadReclamo)src.Prioridad))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => EstadoReclamo.Abierto))
                .ForMember(dest => dest.FechaReclamo, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.NumeroReclamo, opt => opt.Ignore())
                .ForMember(dest => dest.FechaResolucion, opt => opt.Ignore())
                .ForMember(dest => dest.MontoAprobado, opt => opt.Ignore())
                .ForMember(dest => dest.Poliza, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAsignado, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
        }

        private static string FixEncoding(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return input ?? string.Empty;

            return input
                .Replace("Ã¡", "á")
                .Replace("Ã©", "é")
                .Replace("Ã­", "í")
                .Replace("Ã³", "ó")
                .Replace("Ãº", "ú")
                .Replace("Ã±", "ñ")
                .Replace("Ã", "Á")
                .Replace("Ã‰", "É");
        }
    }
}