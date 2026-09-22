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

        CreateMap<Email, EmailDto>()
            .ForMember(dest => dest.Classification, opt => opt.MapFrom(
                src => src.Classification.ToString().ToLowerInvariant()))
            .ForMember(dest => dest.UserFeedback, opt => opt.MapFrom(
                src => src.UserFeedback.HasValue ? src.UserFeedback.Value.ToString().ToLowerInvariant() : null));
    }
}
