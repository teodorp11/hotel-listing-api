using HotelListing.API.Common.Models.Filtering;
using HotelListing.API.Common.Models.Paging;
using HotelListing.API.Common.Results;
using HotelListing.API.DTOs.Hotel;

namespace HotelListing.API.Contracts;

public interface IHotelService
{
    Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto hotelDto);
    Task<Result> DeleteHotelAsync(int id);
    Task<Result<GetHotelDto>> GetHotelAsync(int id);
    Task<Result<PagedResult<GetHotelDto>>> GetHotelsAsync(PaginationParameters paginationParameters, HotelFilterParameters hotelFilterParameters);
    Task<bool> HotelExistsAsync(string name, int countryId);
    Task<Result> UpdateHotelAsync(int id, UpdateHotelDto updateDto);
}