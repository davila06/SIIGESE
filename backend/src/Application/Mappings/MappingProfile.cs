using AutoMapper;
using Domain.Entities;
using Application.DTOs;
using System;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
        // Helper: parse ISO date string "YYYY-MM-DD" from the frontend form into DateTime.
        // Falls back to UtcNow so the mapper never throws on bad input.
        private static DateTime ParseIsoDate(string value) =>
            DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var dt)
                ? dt
                : DateTime.UtcNow;

        public MappingProfile()
        {
            // Cliente mappings
            CreateMap<Cliente, ClienteDto>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.UpdatedBy));
            
            CreateMap<ClienteDto, Cliente>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.UpdatedBy));

            // Poliza mappings
            CreateMap<Poliza, PolizaDto>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.UpdatedBy));

            CreateMap<PolizaDto, Poliza>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.UpdatedBy));

            // CreatePolizaDto → Poliza  (used by Create and Update endpoints from the form)
            // FechaVigencia arrives as ISO string "YYYY-MM-DD" from the frontend date input.
            CreateMap<CreatePolizaDto, Poliza>()
                .ForMember(dest => dest.FechaVigencia, opt => opt.MapFrom(
                    src => ParseIsoDate(src.FechaVigencia)))
                // Audit fields are set explicitly in the service; ignore them here.
                .ForMember(dest => dest.CreatedBy,  opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy,  opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt,  opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt,  opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted,  opt => opt.Ignore())
                .ForMember(dest => dest.EsActivo,   opt => opt.Ignore());

            // Cobro mappings
            CreateMap<Cobro, CobroDto>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
                .ForMember(dest => dest.MetodoPago, opt => opt.MapFrom(src => src.MetodoPago.ToString()));
            
            CreateMap<CobroDto, Cobro>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => Enum.Parse<EstadoCobro>(src.Estado)))
                .ForMember(dest => dest.MetodoPago, opt => opt.MapFrom(src => Enum.Parse<MetodoPago>(src.MetodoPago)));

            // Reclamo mappings
            CreateMap<Reclamo, ReclamoDto>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
                .ForMember(dest => dest.TipoReclamo, opt => opt.MapFrom(src => src.TipoReclamo.ToString()))
                .ForMember(dest => dest.Prioridad, opt => opt.MapFrom(src => src.Prioridad.ToString()));

            CreateMap<ReclamoDto, Reclamo>()
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => Enum.Parse<EstadoReclamo>(src.Estado)))
                .ForMember(dest => dest.TipoReclamo, opt => opt.MapFrom(src => Enum.Parse<TipoReclamo>(src.TipoReclamo)))
                .ForMember(dest => dest.Prioridad, opt => opt.MapFrom(src => Enum.Parse<PrioridadReclamo>(src.Prioridad)));

            // CreateReclamoDto → Reclamo (used when creating a new reclamo from the API)
            // Audit fields, generated fields, and navigation properties are set by the service.
            CreateMap<CreateReclamoDto, Reclamo>()
                .ForMember(dest => dest.Id,                 opt => opt.Ignore())
                .ForMember(dest => dest.NumeroReclamo,      opt => opt.Ignore())
                .ForMember(dest => dest.FechaReclamo,       opt => opt.Ignore())
                .ForMember(dest => dest.FechaResolucion,    opt => opt.Ignore())
                .ForMember(dest => dest.Estado,             opt => opt.Ignore())
                .ForMember(dest => dest.MontoAprobado,      opt => opt.Ignore())
                .ForMember(dest => dest.DocumentosAdjuntos, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt,          opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt,          opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy,          opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy,          opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted,          opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioAsignado,    opt => opt.Ignore())
                .ForMember(dest => dest.Poliza,             opt => opt.Ignore());
        }
    }
}
