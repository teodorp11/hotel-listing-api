namespace HotelListing.API.DTOs.Hotel;

public record GetHotelDto(
    int Id,
    string Name,
    string Address,
    double Rating,
    decimal PerNightRate,
    string CountryName
);
