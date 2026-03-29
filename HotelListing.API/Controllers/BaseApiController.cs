using HotelListing.API.Common.Constants;
using HotelListing.API.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.API.Controllers;

/// <summary>
/// An abstract base controller that provides standardized helper methods for mapping application results and errors to ASP.NET Core ActionResults.
/// </summary>
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Evaluates a generic result object and maps it to either a 200 OK response with the value, or an appropriate error response.
    /// </summary>
    /// <typeparam name="T">The type of the underlying value in the result.</typeparam>
    /// <param name="result">The result object to evaluate.</param>
    /// <returns>An <see cref="ActionResult{T}"/> representing the outcome of the operation.</returns>
    protected ActionResult<T> ToActionResult<T>(Result<T> result)
        => result.IsSuccess ? Ok(result.Value) : MapErrorsToResponse(result.Errors);

    /// <summary>
    /// Evaluates a non-generic result object and maps it to either a 204 No Content response, or an appropriate error response.
    /// </summary>
    /// <param name="result">The result object to evaluate.</param>
    /// <returns>An <see cref="ActionResult"/> representing the outcome of the operation.</returns>
    protected ActionResult ToActionResult(Result result)
        => result.IsSuccess ? NoContent() : MapErrorsToResponse(result.Errors);

    /// <summary>
    /// Maps an array of application errors to a standardized HTTP Problem Details response based on the error code.
    /// </summary>
    /// <param name="errors">An array of errors generated during the execution of a request.</param>
    /// <returns>An <see cref="ActionResult"/> formatted as a Problem Details response (e.g., 400, 404, 403, 409, or 500).</returns>
    protected ActionResult MapErrorsToResponse(Error[] errors)
    {
        if (errors is null || errors.Length == 0)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred",
                detail: "No error details provided"
            );
        }

        var e = errors[0];

        var errorDetails = string.Join("; ", errors.Select(x => x.Description));

        return e.Code switch
        {
            ErrorCodes.NotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Resource not found",
                detail: errorDetails
            ),
            ErrorCodes.Validation => ValidationProblem(
                title: "Validation failed",
                detail: errorDetails
            ),
            ErrorCodes.BadRequest => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad request",
                detail: errorDetails
            ),
            ErrorCodes.Conflict => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: ErrorCodes.Conflict,
                detail: errorDetails
            ),
            ErrorCodes.Forbid => Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden",
                detail: errorDetails
            ),
            _ => Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: string.Join("; ", errors.Select(x => x.Description)),
                title: e.Code
            )
        };
    }
}