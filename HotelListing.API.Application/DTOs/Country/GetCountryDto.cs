using HotelListing.API.DTOs.Hotel;

namespace HotelListing.API.DTOs.Country;

public record GetCountryDto(
    int Id,
    string Name,
    string ShortName,
    List<GetHotelSlimDto>? Hotels
);