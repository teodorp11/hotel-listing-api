using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.API.Controllers;

/// <summary>
/// Version 2 endpoints for managing countries within the Hotel Listing API. 
/// Note: This API version is deprecated.
/// </summary>
[Route("api/v{version:apiVersion}/countries")]
[ApiController]
[ApiVersion("2.0", Deprecated = true)]
public class CountriesV2Controller : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of countries using the enhanced Version 2 implementation.
    /// </summary>
    /// <param name="pageNumber">The specific page number to retrieve. Defaults to 1.</param>
    /// <param name="pageSize">The maximum number of items to return per page. Defaults to 10.</param>
    /// <returns>An object containing versioning information and pagination details.</returns>
    /// <response code="200">Returns the paginated data structure.</response>
    [HttpGet]
    public IActionResult GetCountries(
        [FromQuery] int? pageNumber = 1,
        [FromQuery] int? pageSize = 10)
    {
        // Version 2 implementation with pagination
        return Ok(new
        {
            Version = "2.0",
            Message = "Enhanced countries endpoint with pagination",
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }
}