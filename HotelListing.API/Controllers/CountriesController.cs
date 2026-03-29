using Asp.Versioning;
using HotelListing.API.Common.Constants;
using HotelListing.API.Common.Models.Filtering;
using HotelListing.API.Common.Models.Paging;
using HotelListing.API.Contracts;
using HotelListing.API.Controllers;
using HotelListing.API.DTOs.Country;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HotelListing.Api.Controllers;

/// <summary>
/// Endpoints for managing countries within the Hotel Listing API.
/// </summary>
/// <param name="countriesService">The service used to interact with country data.</param>
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
[EnableRateLimiting(RateLimitingConstants.FixedPolicy)]
public class CountriesController(ICountryService countriesService) : BaseApiController
{
    /// <summary>
    /// Retrieves a paginated and filtered list of all countries.
    /// </summary>
    /// <param name="countryFilterParameters">Parameters to filter and paginate the country results.</param>
    /// <returns>A collection of countries matching the filter criteria.</returns>
    /// <response code="200">Returns the list of countries.</response>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCountriesDto>>> GetCountries([FromQuery] CountryFilterParameters countryFilterParameters)
    {
        var result = await countriesService.GetCountriesAsync(countryFilterParameters);

        return ToActionResult(result);
    }

    /// <summary>
    /// Retrieves a specific country by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the country.</param>
    /// <returns>The country details if found.</returns>
    /// <response code="200">Returns the requested country.</response>
    /// <response code="404">If the country with the specified ID is not found.</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<GetCountryDto>> GetCountry(int id)
    {
        var result = await countriesService.GetCountryAsync(id);

        return ToActionResult(result);
    }

    /// <summary>
    /// Retrieves a paginated list of hotels associated with a specific country.
    /// </summary>
    /// <param name="countryId">The unique identifier of the country.</param>
    /// <param name="paginationParameters">Parameters to paginate the hotel results.</param>
    /// <param name="countryFilterParameters">Parameters to filter the specific country's hotels.</param>
    /// <returns>A list of hotels belonging to the specified country.</returns>
    /// <response code="200">Returns the list of associated hotels.</response>
    /// <response code="404">If the country is not found.</response>
    [HttpGet("{countryId:int}/hotels")]
    public async Task<ActionResult<GetCountryHotelsDto>> GetCountryHotels(
        [FromRoute] int countryId,
        [FromQuery] PaginationParameters paginationParameters,
        [FromQuery] CountryFilterParameters countryFilterParameters)
    {
        var result = await countriesService.GetCountryHotelsAsync(countryId, paginationParameters, countryFilterParameters);

        return ToActionResult(result);
    }

    /// <summary>
    /// Updates the details of an existing country.
    /// </summary>
    /// <param name="id">The unique identifier of the country to update.</param>
    /// <param name="updateDto">The updated country details.</param>
    /// <returns>No content on successful update.</returns>
    /// <response code="204">If the update was successful.</response>
    /// <response code="400">If the provided data is invalid or the ID does not match.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have Administrator privileges.</response>
    /// <response code="404">If the country to update is not found.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> PutCountry(int id, UpdateCountryDto updateDto)
    {
        var result = await countriesService.UpdateCountryAsync(id, updateDto);

        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a new country entry in the system.
    /// </summary>
    /// <param name="createDto">The details of the new country to create.</param>
    /// <returns>The newly created country details along with its route.</returns>
    /// <response code="201">Returns the newly created country.</response>
    /// <response code="400">If the provided data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have Administrator privileges.</response>
    [HttpPost]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<ActionResult<GetCountryDto>> PostCountry(CreateCountryDto createDto)
    {
        var result = await countriesService.CreateCountryAsync(createDto);

        if (!result.IsSuccess)
        {
            return MapErrorsToResponse(result.Errors);
        }

        return CreatedAtAction(nameof(GetCountry), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Removes a specific country from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the country to delete.</param>
    /// <returns>No content on successful deletion.</returns>
    /// <response code="204">If the deletion was successful.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have Administrator privileges.</response>
    /// <response code="404">If the country to delete is not found.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        var result = await countriesService.DeleteCountryAsync(id);

        return ToActionResult(result);
    }

    /// <summary>
    /// Partially updates a specific country using a JSON Patch document.
    /// </summary>
    /// <param name="id">The unique identifier of the country to patch.</param>
    /// <param name="PatchDocument">The JSON Patch document containing the operations to apply.</param>
    /// <returns>No content on successful patch application.</returns>
    /// <response code="204">If the patch was applied successfully.</response>
    /// <response code="400">If the patch document is null or invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have Administrator privileges.</response>
    /// <response code="404">If the country to patch is not found.</response>
    [HttpPatch("{id}")]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> PatchCountry(int id, [FromBody] JsonPatchDocument<UpdateCountryDto> PatchDocument)
    {
        if (PatchDocument == null)
        {
            return BadRequest("Patch document is required.");
        }

        var result = await countriesService.PatchCountryAsync(id, PatchDocument);

        return ToActionResult(result);
    }
}