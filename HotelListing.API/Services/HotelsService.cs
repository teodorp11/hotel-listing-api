using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.DTOs.Hotel;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Services;

public class HotelsService(HotelListingDbContext context) : IHotelsService
{
    public async Task<IEnumerable<GetHotelsDto>> GetHotelsAsync()
    {
        return await context.Hotels
            .Select(h => new GetHotelsDto(h.Id, h.Name, h.Address, h.Rating, h.CountryId))
            .ToListAsync();
    }

    public async Task<GetHotelDto?> GetHotelAsync(int id)
    {
        return await context.Hotels
            .Where(h => h.Id == id)
            .Select(h => new GetHotelDto(
                h.Id,
                h.Name,
                h.Address,
                h.Rating,
                h.Country!.Name
             ))
            .FirstOrDefaultAsync();
    }

    public async Task UpdateHotelAsync(int id, UpdateHotelDto hotelDto)
    {
        var hotel = await context.Hotels.FindAsync(id) ?? throw new KeyNotFoundException("Country not found");

        hotel.Name = hotelDto.Name;
        hotel.Address = hotelDto.Address;
        hotel.Rating = hotelDto.Rating;
        hotel.CountryId = hotelDto.CountryId;

        context.Hotels.Update(hotel);

        await context.SaveChangesAsync();
    }

    public async Task<GetHotelSlimDto> CreateHotelAsync(CreateHotelDto createDto)
    {
        var hotel = new Hotel
        {
            Name = createDto.Name,
            Address = createDto.Address,
            Rating = createDto.Rating,
            CountryId = createDto.CountryId,
        };

        context.Hotels.Add(hotel);

        await context.SaveChangesAsync();

        return new GetHotelSlimDto(
            hotel.Id,
            hotel.Name,
            hotel.Address,
            hotel.Rating
        );
    }

    public async Task DeleteHotelAsync(int id)
    {
        var hotel = await context.Hotels.FindAsync(id) ?? throw new KeyNotFoundException("Country not found");

        context.Hotels.Remove(hotel);

        await context.SaveChangesAsync();
    }
    public async Task<bool> HotelExists(int id)
    {
        return await context.Hotels.AnyAsync(h => h.Id == id);
    }

    public async Task<bool> HotelExists(string name)
    {
        return await context.Hotels.AnyAsync(h => h.Name == name);
    }
}
