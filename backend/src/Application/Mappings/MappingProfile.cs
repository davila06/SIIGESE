using AutoMapper;
using Domain.Entities;
using Application.DTOs;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
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
        }
    }
}
