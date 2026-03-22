using HotelListing.API.DTOs.Booking;
using HotelListing.API.Results;

namespace HotelListing.API.Contracts;

public interface IBookingService
{
    Task<Result<IEnumerable<GetBookingDto>>> GetBookingsForHotelAsync(int hotelId);
    Task<Result<GetBookingDto>> CreateBookingAsync(CreateBookingDto createDto);
    Task<Result<GetBookingDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto updateBookingDto);
    Task<Result> CancelBookingAsync(int hotelId, int bookingId);
    Task<Result> AdminCancelBookingAsync(int hotelId, int bookingId);
    Task<Result> AdminConfirmBookingAsync(int hotelId, int bookingId);
    Task<Result<IEnumerable<GetBookingDto>>> GetUserBookingsForHotelAsync(int hotelId);
}