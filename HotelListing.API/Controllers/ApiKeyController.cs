using HotelListing.API.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.API.Controllers;

/// <summary>
/// A sample controller used to demonstrate and test API Key authentication.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = AuthenticationDefaults.ApiKeyScheme)]
public class ApiKeyController : ControllerBase
{
    /// <summary>
    /// Retrieves a list of sample values. Requires a valid API Key.
    /// </summary>
    /// <returns>An array of sample strings.</returns>
    /// <response code="200">Returns the list of sample values.</response>
    /// <response code="401">If the API Key is missing or invalid.</response>
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    /// <summary>
    /// Retrieves a specific sample value by its identifier. Requires a valid API Key.
    /// </summary>
    /// <param name="id">The unique identifier of the sample value.</param>
    /// <returns>A sample string value.</returns>
    /// <response code="200">Returns the sample value.</response>
    /// <response code="401">If the API Key is missing or invalid.</response>
    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    /// <summary>
    /// Submits a new sample value to the server. Requires a valid API Key.
    /// </summary>
    /// <param name="value">The sample string value to create.</param>
    /// <response code="200">If the value was successfully processed.</response>
    /// <response code="400">If the provided value is invalid.</response>
    /// <response code="401">If the API Key is missing or invalid.</response>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    /// <summary>
    /// Updates an existing sample value on the server. Requires a valid API Key.
    /// </summary>
    /// <param name="id">The unique identifier of the value to update.</param>
    /// <param name="value">The updated string value.</param>
    /// <response code="200">If the value was successfully updated.</response>
    /// <response code="400">If the provided value or ID is invalid.</response>
    /// <response code="401">If the API Key is missing or invalid.</response>
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    /// <summary>
    /// Deletes a specific sample value from the server. Requires a valid API Key.
    /// </summary>
    /// <param name="id">The unique identifier of the value to delete.</param>
    /// <response code="200">If the value was successfully deleted.</response>
    /// <response code="401">If the API Key is missing or invalid.</response>
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}