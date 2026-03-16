using HotelListing.Api.DTOs.Auth;
using HotelListing.API.Constants;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.DTOs.Auth;
using HotelListing.API.Results;
using Microsoft.AspNetCore.Identity;

namespace HotelListing.API.Services
{
    public class UsersService(UserManager<ApplicationUser> userManager) : IUsersService
    {
        public async Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto)
        {
            var user = new ApplicationUser
            {
                Email = registerUserDto.Email,
                FirstName = registerUserDto.FirstName,
                LastName = registerUserDto.LastName,
                UserName = registerUserDto.Email
            };

            var result = await userManager.CreateAsync(user, registerUserDto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => new Error(ErrorCodes.BadRequest, e.Description)).ToArray();
                return Result<RegisteredUserDto>.BadRequest(errors);
            }

            var registeredUser = new RegisteredUserDto
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Id = user.Id,
                Role = registerUserDto.Role,
            };

            // Optional: Send confirmation Email
            return Result<RegisteredUserDto>.Success(registeredUser);
        }

        public async Task<Result<string>> LoginAsync(LoginUserDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);

            if (user is null)
            {
                return Result<string>.Failure(new Error(ErrorCodes.BadRequest, "Invalid credentials."));
            }

            var valid = await userManager.CheckPasswordAsync(user, dto.Password);

            if (!valid)
            {
                return Result<string>.Failure(new Error(ErrorCodes.BadRequest, "Invalid credentials."));
            }

            return Result<string>.Success("Login succesful.");
        }
    }
}
