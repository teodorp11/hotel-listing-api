using AutoMapper;
using HotelListing.API.Data;
using HotelListing.API.DTOs.Hotel;

namespace HotelListing.API.MappingProfiles;

public class HotelMappingProfile : Profile
{
    public HotelMappingProfile()
    {
        CreateMap<Hotel, GetHotelsDto>();

        CreateMap<Hotel, GetHotelDto>()
        .ForMember(d => d.Country, config => config.MapFrom<CountryNameResolver>());

        CreateMap<UpdateHotelDto, Hotel>();
        
        CreateMap<CreateHotelDto, Hotel>();

        CreateMap<Hotel, GetHotelSlimDto>();
    }
}

public class CountryNameResolver : IValueResolver<Hotel, GetHotelDto, string>
{
    public string Resolve(Hotel source, GetHotelDto destination, string destMember, ResolutionContext context)
    {
        return source.Country?.Name ?? string.Empty;
    }
}