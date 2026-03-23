using AutoMapper;
using HotelListing.API.Domain;
using HotelListing.API.DTOs.Country;

namespace HotelListing.API.MappingProfiles;

public class CountryMappingProfile : Profile
{
    public CountryMappingProfile()
    {
        CreateMap<Country, GetCountryDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id));
        CreateMap<Country, GetCountriesDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id));
        CreateMap<CreateCountryDto, Country>();
    }
}