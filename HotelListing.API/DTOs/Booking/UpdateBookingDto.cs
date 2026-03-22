namespace HotelListing.API.DTOs.Booking;

public record UpdateBookingDto
(
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Guests
);