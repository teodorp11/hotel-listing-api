using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.API.Common.Constants;
using HotelListing.API.Common.Results;
using HotelListing.API.Contracts;
using HotelListing.API.Domain;
using HotelListing.API.Domain.Enums;
using HotelListing.API.DTOs.Booking;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Services;

public class BookingService(HotelListingDbContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper, IUserService userService) : IBookingService
{
    public async Task<Result<IEnumerable<GetBookingDto>>> GetBookingsForHotelAsync(int hotelId)
    {
        var hotelExits = await context.Hotels.AnyAsync(h => h.Id == hotelId);

        if (!hotelExits)
        {
            return Result<IEnumerable<GetBookingDto>>.Failure(new Error(ErrorCodes.NotFound, $"Hotel {hotelId} was not found."));
        }

        var bookings = await context.Bookings
            .Where(b => b.HotelId == hotelId)
            .ProjectTo<GetBookingDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<IEnumerable<GetBookingDto>>.Success(bookings);
    }

    public async Task<Result<GetBookingDto>> CreateBookingAsync(CreateBookingDto createDto)
    {
        var userId = userService.UserId;
        
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.Validation, "User is required."));
        }

        bool overlaps = await IsOverlap(createDto.HotelId, userId, createDto.CheckIn, createDto.CheckOut);

        if (overlaps)
        {
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.Conflict, "The selected dates overlap with an existing booking."));
        }
        
        var hotel = await context.Hotels
            .Where(h => h.Id == createDto.HotelId)
            .FirstOrDefaultAsync();
        
        if (hotel is null)
        {
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.NotFound, $"Hotel {createDto.HotelId} was not found."));
        }

        var nights = createDto.CheckOut.DayNumber - createDto.CheckIn.DayNumber;

        if (nights <= 0)
        {
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.Validation, "Check-out muest be after Check-in."));
        }

        if (createDto.Guests <= 0)
        {
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.Validation, "Number of guests must be at least equal to one."));
        }

        var totalPrice = hotel.PerNightRate * nights;

        var booking = mapper.Map<Booking>(createDto);

        booking.UserId = userId;

        context.Bookings.Add(booking);

        await context.SaveChangesAsync();

        var result = mapper.Map<GetBookingDto>(booking);

        return Result<GetBookingDto>.Success(result);
    }

    public async Task<Result<GetBookingDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto updateDto)
    {
        var userId = userService.UserId;

        bool overlaps = await IsOverlap(hotelId, userId, updateDto.CheckIn, updateDto.CheckOut, bookingId);

        if (overlaps)
        {
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.Conflict, "The selected dates overlap with an existing booking."));
        }

        var booking = await context.Bookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b =>
                b.Id == bookingId
                && b.HotelId == hotelId
                && b.UserId == userId);

        if (booking is null)
        {
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}' was not found."));
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.Conflict, "Cancelled bookings cannot be modified."));
        }

        mapper.Map(updateDto, booking);

        var perNight = booking.Hotel!.PerNightRate;
        
        var nights = updateDto.CheckOut.DayNumber - updateDto.CheckIn.DayNumber;

        booking.TotalPrice = perNight * nights;
        
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync();

        var result = mapper.Map<GetBookingDto>(booking);

        return Result<GetBookingDto>.Success(result);
    }

    public async Task<Result> CancelBookingAsync(int hotelId, int bookingId)
    {
        var userId = userService.UserId;

        var booking = await context.Bookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b =>
                b.Id == bookingId
                && b.HotelId == hotelId
                && b.UserId == userId);

        if (booking is null)
        {
            return Result.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}' was not found."));
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result.Failure(new Error(ErrorCodes.Conflict, "This booking has already been cancelled."));
        }

        booking.Status = BookingStatus.Cancelled;
        
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> AdminCancelBookingAsync(int hotelId, int bookingId)
    {
        var userId = userService.UserId;

        var isHotelAdminUser = await context.HotelAdmins
            .AnyAsync(q => q.UserId == userId && q.HotelId == hotelId);

        if (!isHotelAdminUser)
        {
            return Result.Failure(new Error(ErrorCodes.Forbid, $"You are not one of the admins of hotel {hotelId}."));
        }

        var booking = await context.Bookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b =>
                b.Id == bookingId
                && b.HotelId == hotelId);

        if (booking is null)
        {
            return Result.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}' was not found."));
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result.Failure(new Error(ErrorCodes.Conflict, "This booking has already been cancelled."));
        }

        booking.Status = BookingStatus.Cancelled;
        
        booking.UpdatedAtUtc = DateTime.UtcNow;
        
        await context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> AdminConfirmBookingAsync(int hotelId, int bookingId)
    {
        var userId = userService.UserId;

        var isHotelAdminUser = await context.HotelAdmins
            .AnyAsync(q => q.UserId == userId && q.HotelId == hotelId);

        if (!isHotelAdminUser)
        {
            return Result.Failure(new Error(ErrorCodes.Forbid, $"You are not one of the admins of hotel {hotelId}."));
        }

        var booking = await context.Bookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b =>
                b.Id == bookingId
                && b.HotelId == hotelId);

        if (booking is null)
        {
            return Result.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}' was not found."));
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result.Failure(new Error(ErrorCodes.Conflict, "This booking has already been cancelled."));
        }

        booking.Status = BookingStatus.Confirmed;

        booking.UpdatedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<IEnumerable<GetBookingDto>>> GetUserBookingsForHotelAsync(int hotelId)
    {
        var userId = userService.UserId;

        var hotelExists = await context.Hotels.AnyAsync(h => h.Id == hotelId);
        
        if (!hotelExists)
        {
            return Result<IEnumerable<GetBookingDto>>.Failure(new Error(ErrorCodes.NotFound, $"Hotel '{hotelId}' was not found."));
        }

        var bookings = await context.Bookings
            .Where(b => b.HotelId == hotelId && b.UserId == userId)
            .OrderBy(b => b.CheckIn)
            .ProjectTo<GetBookingDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<IEnumerable<GetBookingDto>>.Success(bookings);
    }

    private async Task<bool> IsOverlap(int hotelId, string userId, DateOnly checkIn, DateOnly checkOut, int? bookingId = null)
    {
        var query = context.Bookings
            .Where(
                    b => b.HotelId == hotelId
                    && b.Status != BookingStatus.Cancelled
                    && checkIn < b.CheckOut
                    && checkOut > b.CheckIn
                    && b.UserId == userId)
            .AsQueryable();

        if (bookingId.HasValue)
        {
            query = query.Where(q => q.Id != bookingId.Value);
        }

        return await query.AnyAsync();
    }
}