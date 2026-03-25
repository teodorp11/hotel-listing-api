using HotelListing.API.Common.Constants;
using HotelListing.API.Common.Models.Config;
using HotelListing.API.Common.Results;
using HotelListing.API.Contracts;
using HotelListing.API.Domain;
using HotelListing.API.DTOs.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelListing.API.Services
{
    public class UserService(UserManager<ApplicationUser> userManager, HotelListingDbContext hotelListingDbContext, IOptions<JwtSettings> jwtOptions, IHttpContextAccessor
        httpContextAccessor, ILogger<UserService> logger) : IUserService
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

                logger.LogError("User registration failed for {Email}: {Errors}", registerUserDto.Email, string.Join(", ", errors));

                return Result<RegisteredUserDto>.BadRequest(errors);
            }

            await userManager.AddToRoleAsync(user, registerUserDto.Role);

            if (registerUserDto.Role == RoleNames.HotelAdmin)
            {
                var hotelAdmin = hotelListingDbContext.HotelAdmins.Add(
                    new HotelAdmin
                    {
                        UserId = user.Id,
                        HotelId = registerUserDto.AssociatedHotelId.GetValueOrDefault()
                    });

                await hotelListingDbContext.SaveChangesAsync();
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
                logger.LogWarning("Failed login attempt for email: {Email}", dto.Email);
                
                return Result<string>.Failure(new Error(ErrorCodes.BadRequest, "Invalid credentials."));
            }

            var valid = await userManager.CheckPasswordAsync(user, dto.Password);

            if (!valid)
            {
                return Result<string>.Failure(new Error(ErrorCodes.BadRequest, "Invalid credentials."));
            }

            // Issue a token
            var token = await GenerateToken(user);

            return Result<string>.Success(token);
        }

        public string UserId => httpContextAccessor?
            .HttpContext?
            .User?
            .FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? httpContextAccessor?
            .HttpContext?
            .User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? string.Empty;

        private async Task<string> GenerateToken(ApplicationUser user)
        {
            // Set basic user claims
            var claims = new List<Claim>
            {
                new (JwtRegisteredClaimNames.Sub, user.Id),
                new (JwtRegisteredClaimNames.Email, user.Email),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new (JwtRegisteredClaimNames.Name, user.FullName)
            };

            // Set user role claims
            var roles = await userManager.GetRolesAsync(user);
            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();

            claims = claims.Union(roleClaims).ToList();

            // Set JWT Key credentials
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Create an encoded token
            var token = new JwtSecurityToken(
                issuer: jwtOptions.Value.Issuer,
                audience: jwtOptions.Value.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(jwtOptions.Value.DurationInMinutes)),
                signingCredentials: credentials
                );

            // Return token value
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
