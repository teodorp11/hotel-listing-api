using HotelListing.API.DTOs.Hotel;

namespace HotelListing.API.Contracts
{
    public interface IHotelsService
    {
        Task<GetHotelSlimDto> CreateHotelAsync(CreateHotelDto createDto);
        Task DeleteHotelAsync(int id);
        Task<GetHotelDto?> GetHotelAsync(int id);
        Task<IEnumerable<GetHotelsDto>> GetHotelsAsync();
        Task<bool> HotelExists(int id);
        Task<bool> HotelExists(string name);
        Task UpdateHotelAsync(int id, UpdateHotelDto hotelDto);
    }
}