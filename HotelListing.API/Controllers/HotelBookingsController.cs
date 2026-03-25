using HotelListing.Api.AuthorizationFilters;
using HotelListing.API.Common.Models.Filtering;
using HotelListing.API.Common.Models.Paging;
using HotelListing.API.Contracts;
using HotelListing.API.DTOs.Booking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HotelListing.API.Controllers;

[Route("api/hotels/{hotelId:int}/bookings")]
[ApiController]
[Authorize]
[EnableRateLimiting("perUser")]
public class HotelBookingsController(IBookingService bookingService) : BaseApiController
{
    // "api/hotels/3/bookings"
    [HttpGet]
    public async Task<ActionResult<PagedResult<GetBookingDto>>> GetBookings([FromRoute] int hotelId, [FromQuery] PaginationParameters paginationParameters, [FromQuery] BookingFilterParameters bookingFilterParameters)
    {
        var result = await bookingService.GetUserBookingsForHotelAsync(hotelId, paginationParameters, bookingFilterParameters);

        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<GetBookingDto>> CreateBooking([FromRoute] int hotelId, [FromBody] CreateBookingDto createBookingDto)
    {
        var result = await bookingService.CreateBookingAsync(createBookingDto);

        return ToActionResult(result);
    }

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

    [HttpPut("{bookingId:int}/cancel")]
    public async Task<ActionResult<GetBookingDto>> CancelBooking(
    [FromRoute] int hotelId,
    [FromRoute] int bookingId
    )
    {
        var result = await bookingService.CancelBookingAsync(hotelId, bookingId);

        return ToActionResult(result);
    }

    [HttpGet("admin")]
    [HotelOrSystemAdminAttribute]
    public async Task<ActionResult<PagedResult<GetBookingDto>>> GetBookingsAdmin([FromRoute] int hotelId, [FromQuery] PaginationParameters paginationParameters, [FromQuery] BookingFilterParameters bookingFilterParameters)
    {
        var result = await bookingService.GetBookingsForHotelAsync(hotelId, paginationParameters, bookingFilterParameters);
        
        return ToActionResult(result);
    }

    [HttpPut("{bookingId:int}/admin/cancel")]
    [HotelOrSystemAdminAttribute]
    public async Task<IActionResult> AdminCancelBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await bookingService.AdminCancelBookingAsync(hotelId, bookingId);
        
        return ToActionResult(result);
    }

    [HttpPut("{bookingId:int}/admin/confirm")]
    [HotelOrSystemAdminAttribute]
    public async Task<IActionResult> AdminConfirmBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await bookingService.AdminConfirmBookingAsync(hotelId, bookingId);
        
        return ToActionResult(result);
    }
}