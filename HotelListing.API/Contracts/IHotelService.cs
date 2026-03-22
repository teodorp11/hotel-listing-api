using HotelListing.API.DTOs.Hotel;
using HotelListing.API.Results;

namespace HotelListing.API.Contracts;

public interface IHotelService
{
    Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto hotelDto);
    Task<Result> DeleteHotelAsync(int id);
    Task<Result<GetHotelDto>> GetHotelAsync(int id);
    Task<Result<IEnumerable<GetHotelsDto>>> GetHotelsAsync();
    Task<bool> HotelExistsAsync(string name, int countryId);
    Task<Result> UpdateHotelAsync(int id, UpdateHotelDto updateDto);
}