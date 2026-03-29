using HotelListing.API.DTOs.Auth;
using HotelListing.API.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.API.Controllers;

/// <summary>
/// Endpoints for user authentication, registration, and token generation.
/// </summary>
/// <param name="usersService">The service used to manage user accounts and authentication.</param>
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController(IUserService usersService) : BaseApiController
{
    /// <summary>
    /// Registers a new user account in the system.
    /// </summary>
    /// <param name="registerUserDto">The details of the new user to register.</param>
    /// <returns>The newly registered user details.</returns>
    /// <response code="200">If the registration was successful.</response>
    /// <response code="400">If the provided registration data is invalid or the email is already taken.</response>
    [HttpPost("register")]
    public async Task<ActionResult<RegisteredUserDto>> Register(RegisterUserDto registerUserDto)
    {
        var result = await usersService.RegisterAsync(registerUserDto);

        return ToActionResult(result);
    }

    /// <summary>
    /// Authenticates a user and generates a JWT token for accessing protected endpoints.
    /// </summary>
    /// <param name="loginUserDto">The user's login credentials (email and password).</param>
    /// <returns>A JWT Bearer token.</returns>
    /// <response code="200">Returns the JWT token upon successful authentication.</response>
    /// <response code="400">If the login request payload is invalid.</response>
    /// <response code="401">If the authentication fails due to incorrect credentials.</response>
    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(LoginUserDto loginUserDto)
    {
        var result = await usersService.LoginAsync(loginUserDto);

        return ToActionResult(result);
    }
}