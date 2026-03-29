using Microsoft.AspNetCore.Mvc;
using HotelListing.API.DTOs.Hotel;
using HotelListing.API.Contracts;
using Microsoft.AspNetCore.Authorization;
using HotelListing.API.Common.Models.Paging;
using HotelListing.API.Common.Models.Filtering;

namespace HotelListing.API.Controllers;

/// <summary>
/// Endpoints for managing hotels within the Hotel Listing API.
/// </summary>
/// <param name="hotelsService">The service used to interact with hotel data.</param>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class HotelsController(IHotelService hotelsService) : BaseApiController
{
    /// <summary>
    /// Retrieves a paginated and filtered list of all hotels.
    /// </summary>
    /// <param name="paginationParameters">Parameters to paginate the hotel results.</param>
    /// <param name="hotelFilterParameters">Parameters to filter the hotel results (e.g., rating, price).</param>
    /// <returns>A collection of hotels matching the filter criteria.</returns>
    /// <response code="200">Returns the list of hotels.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    public async Task<ActionResult<PagedResult<GetHotelDto>>> GetHotels([FromQuery] PaginationParameters paginationParameters, [FromQuery] HotelFilterParameters hotelFilterParameters)
    {
        var result = await hotelsService.GetHotelsAsync(paginationParameters, hotelFilterParameters);

        return ToActionResult(result);
    }

    /// <summary>
    /// Retrieves a specific hotel by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the hotel.</param>
    /// <returns>The hotel details if found.</returns>
    /// <response code="200">Returns the requested hotel.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the hotel with the specified ID is not found.</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<GetHotelDto>> GetHotel(int id)
    {
        var result = await hotelsService.GetHotelAsync(id);

        return ToActionResult(result);
    }

    /// <summary>
    /// Updates the details of an existing hotel.
    /// </summary>
    /// <param name="id">The unique identifier of the hotel to update.</param>
    /// <param name="hotelDto">The updated hotel details.</param>
    /// <returns>No content on successful update.</returns>
    /// <response code="204">If the update was successful.</response>
    /// <response code="400">If the provided data is invalid or the ID does not match.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have Administrator privileges.</response>
    /// <response code="404">If the hotel to update is not found.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> PutHotel(int id, UpdateHotelDto hotelDto)
    {
        var result = await hotelsService.UpdateHotelAsync(id, hotelDto);

        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a new hotel entry in the system.
    /// </summary>
    /// <param name="hotelDto">The details of the new hotel to create.</param>
    /// <returns>The newly created hotel details along with its route.</returns>
    /// <response code="201">Returns the newly created hotel.</response>
    /// <response code="400">If the provided data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have Administrator privileges.</response>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<GetHotelDto>> PostHotel(CreateHotelDto hotelDto)
    {
        var result = await hotelsService.CreateHotelAsync(hotelDto);

        if (!result.IsSuccess)
        {
            return MapErrorsToResponse(result.Errors);
        }

        return CreatedAtAction("GetHotel", new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Removes a specific hotel from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the hotel to delete.</param>
    /// <returns>No content on successful deletion.</returns>
    /// <response code="204">If the deletion was successful.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have Administrator privileges.</response>
    /// <response code="404">If the hotel to delete is not found.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteHotel(int id)
    {
        var result = await hotelsService.DeleteHotelAsync(id);

        return ToActionResult(result);
    }
}