using Application.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class CotizacionMappingProfile : Profile
    {
        public CotizacionMappingProfile()
        {
            // Mapeo de entidad a DTO
            CreateMap<Cotizacion, CotizacionDto>();

            // Mapeo de DTO de creación a entidad
            CreateMap<CreateCotizacionDto, Cotizacion>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.NumeroCotizacion, opt => opt.Ignore())
                .ForMember(dest => dest.FechaCotizacion, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => "PENDIENTE"))
                .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.FechaActualizacion, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
                .ForMember(dest => dest.Usuario, opt => opt.Ignore());

            // Mapeo de DTO de actualización a entidad
            CreateMap<UpdateCotizacionDto, Cotizacion>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.NumeroCotizacion, opt => opt.Ignore())
                .ForMember(dest => dest.FechaCotizacion, opt => opt.Ignore())
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
                .ForMember(dest => dest.FechaActualizacion, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
                .ForMember(dest => dest.Usuario, opt => opt.Ignore());
        }
    }
}