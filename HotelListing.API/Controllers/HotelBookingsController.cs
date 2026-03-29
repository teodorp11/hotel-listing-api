using HotelListing.Api.AuthorizationFilters;
using HotelListing.API.Common.Models.Filtering;
using HotelListing.API.Common.Models.Paging;
using HotelListing.API.Contracts;
using HotelListing.API.DTOs.Booking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HotelListing.API.Controllers;

/// <summary>
/// Endpoints for managing bookings for a specific hotel.
/// </summary>
/// <param name="bookingService">The service used to interact with booking data.</param>
[Route("api/hotels/{hotelId:int}/bookings")]
[ApiController]
[Authorize]
[EnableRateLimiting("perUser")]
public class HotelBookingsController(IBookingService bookingService) : BaseApiController
{
    /// <summary>
    /// Retrieves a paginated and filtered list of bookings belonging to the currently authenticated user for a specific hotel.
    /// </summary>
    /// <param name="hotelId">The unique identifier of the hotel.</param>
    /// <param name="paginationParameters">Parameters to paginate the booking results.</param>
    /// <param name="bookingFilterParameters">Parameters to filter the bookings.</param>
    /// <returns>A collection of the user's bookings matching the criteria.</returns>
    /// <response code="200">Returns the list of bookings.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    public async Task<ActionResult<PagedResult<GetBookingDto>>> GetBookings([FromRoute] int hotelId, [FromQuery] PaginationParameters paginationParameters, [FromQuery] BookingFilterParameters bookingFilterParameters)
    {
        var result = await bookingService.GetUserBookingsForHotelAsync(hotelId, paginationParameters, bookingFilterParameters);

        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a new booking at the specified hotel for the authenticated user.
    /// </summary>
    /// <param name="hotelId">The unique identifier of the hotel.</param>
    /// <param name="createBookingDto">The details of the booking to create.</param>
    /// <returns>The newly created booking details.</returns>
    /// <response code="200">Returns the newly created booking.</response>
    /// <response code="400">If the provided data is invalid, dates overlap, or hotel does not exist.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost]
    public async Task<ActionResult<GetBookingDto>> CreateBooking([FromRoute] int hotelId, [FromBody] CreateBookingDto createBookingDto)
    {
        var result = await bookingService.CreateBookingAsync(createBookingDto);

        return ToActionResult(result);
    }

    /// <summary>
    /// Updates an existing booking belonging to the authenticated user.
    /// </summary>
    /// <param name="hotelId">The unique identifier of the hotel.</param>
    /// <param name="bookingId">The unique identifier of the booking to update.</param>
    /// <param name="updateBookingDto">The updated booking details.</param>
    /// <returns>The updated booking details.</returns>
    /// <response code="200">Returns the updated booking.</response>
    /// <response code="400">If the provided data is invalid or dates overlap.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the booking is not found or does not belong to the user.</response>
    [HttpPut("{bookingId:int}")]
    public async Task<ActionResult<GetBookingDto>> UpdateBooking(
    [FromRoute] int hotelId,
    [FromRoute] int bookingId,
    [FromBody] UpdateBookingDto updateBookingDto
    )
    {
        var result = await bookingService.UpdateBookingAsync(hotelId, bookingId, updateBookingDto);

        return ToActionResult(result);
    }

    /// <summary>
    /// Cancels an existing booking belonging to the authenticated user.
    /// </summary>
    /// <param name="hotelId">The unique identifier of the hotel.</param>
    /// <param name="bookingId">The unique identifier of the booking to cancel.</param>
    /// <returns>The status of the cancelled booking.</returns>
    /// <response code="200">If the booking was successfully cancelled.</response>
    /// <response code="400">If the booking cannot be cancelled.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the booking is not found or does not belong to the user.</response>
    [HttpPut("{bookingId:int}/cancel")]
    public async Task<ActionResult<GetBookingDto>> CancelBooking(
    [FromRoute] int hotelId,
    [FromRoute] int bookingId
    )
    {
        var result = await bookingService.CancelBookingAsync(hotelId, bookingId);

        return ToActionResult(result);
    }

    /// <summary>
    /// Retrieves all bookings for a specific hotel (Admin only).
    /// </summary>
    /// <param name="hotelId">The unique identifier of the hotel.</param>
    /// <param name="paginationParameters">Parameters to paginate the booking results.</param>
    /// <param name="bookingFilterParameters">Parameters to filter the bookings.</param>
    /// <returns>A collection of all bookings for the hotel.</returns>
    /// <response code="200">Returns the list of bookings.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not an administrator for this hotel.</response>
    [HttpGet("admin")]
    [HotelOrSystemAdminAttribute]
    public async Task<ActionResult<PagedResult<GetBookingDto>>> GetBookingsAdmin([FromRoute] int hotelId, [FromQuery] PaginationParameters paginationParameters, [FromQuery] BookingFilterParameters bookingFilterParameters)
    {
        var result = await bookingService.GetBookingsForHotelAsync(hotelId, paginationParameters, bookingFilterParameters);

        return ToActionResult(result);
    }

    /// <summary>
    /// Cancels any user's booking for the specified hotel (Admin only).
    /// </summary>
    /// <param name="hotelId">The unique identifier of the hotel.</param>
    /// <param name="bookingId">The unique identifier of the booking to cancel.</param>
    /// <returns>No content on successful cancellation.</returns>
    /// <response code="200">If the booking was successfully cancelled.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not an administrator for this hotel.</response>
    /// <response code="404">If the booking is not found.</response>
    [HttpPut("{bookingId:int}/admin/cancel")]
    [HotelOrSystemAdminAttribute]
    public async Task<IActionResult> AdminCancelBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await bookingService.AdminCancelBookingAsync(hotelId, bookingId);

        return ToActionResult(result);
    }

    /// <summary>
    /// Confirms a pending booking for the specified hotel (Admin only).
    /// </summary>
    /// <param name="hotelId">The unique identifier of the hotel.</param>
    /// <param name="bookingId">The unique identifier of the booking to confirm.</param>
    /// <returns>No content on successful confirmation.</returns>
    /// <response code="200">If the booking was successfully confirmed.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not an administrator for this hotel.</response>
    /// <response code="404">If the booking is not found.</response>
    [HttpPut("{bookingId:int}/admin/confirm")]
    [HotelOrSystemAdminAttribute]
    public async Task<IActionResult> AdminConfirmBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await bookingService.AdminConfirmBookingAsync(hotelId, bookingId);

        return ToActionResult(result);
    }
}