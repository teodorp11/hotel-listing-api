using HotelListing.API.DTOs.Auth;
using HotelListing.API.Results;

namespace HotelListing.API.Contracts;

public interface IUsersService
{
    Task<Result<string>> LoginAsync(LoginUserDto dto);
    Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto);
}