using HotelListing.API.Common.Results;
using HotelListing.API.DTOs.Auth;

namespace HotelListing.API.Contracts;

public interface IUserService
{
    string UserId { get; }

    Task<Result<string>> LoginAsync(LoginUserDto dto);
    Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto);
}