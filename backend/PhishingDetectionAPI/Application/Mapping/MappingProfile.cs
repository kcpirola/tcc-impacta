namespace PhishingDetectionAPI.Application.Mapping;

using AutoMapper;
using PhishingDetectionAPI.Application.Dtos;
using PhishingDetectionAPI.Domain.Entities;

/// <summary>
/// Perfil de mapeamento do AutoMapper entre entidades de domínio e DTOs.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
    }
}
