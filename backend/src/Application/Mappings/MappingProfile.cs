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
        }
    }
}