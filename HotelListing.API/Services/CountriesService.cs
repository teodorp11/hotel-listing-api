using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.API.Constants;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.DTOs.Country;
using HotelListing.API.Results;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Services;

public class CountriesService(HotelListingDbContext context, IMapper mapper) : ICountriesService
{
    public async Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync()
    {
        var countries = await context.Countries
            .ProjectTo<GetCountriesDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<IEnumerable<GetCountriesDto>>.Success(countries);
    }
    public async Task<Result<GetCountryDto>> GetCountryAsync(int id)
    {
        var country = await context.Countries
            .Where(c => c.Id == id)
            .ProjectTo<GetCountryDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        return country is null
            ? Result<GetCountryDto>.NotFound()
            : Result<GetCountryDto>.Success(country);
    }

    public async Task<Result<GetCountryDto>> CreateCountryAsync(CreateCountryDto createDto)
    {
        try
        {
            var exists = await CountryExistsAsync(createDto.Name);

            if (exists)
            {
                return Result<GetCountryDto>.Failure(new Error(ErrorCodes.Conflict, $"Country with name {createDto.Name} already exists."));
            }

            var country = mapper.Map<Country>(createDto);

            context.Countries.Add(country);

            await context.SaveChangesAsync();

            var countryDto = mapper.Map<GetCountryDto>(country);

            return Result<GetCountryDto>.Success(countryDto);
        }
        catch (Exception)
        {
            return Result<GetCountryDto>.Failure();
        }
    }

    public async Task<Result> UpdateCountryAsync(int id, UpdateCountryDto updateDto)
    {
        try
        {
            if (id != updateDto.Id)
            {
                return Result.BadRequest(new Error(ErrorCodes.Validation, "Route ID does not match the payload ID."));
            }

            var country = await context.Countries.FindAsync(id);

            if (country is null)
            {
                return Result.NotFound(new Error(ErrorCodes.NotFound, $"Country with ID {id} was not found."));
            }

            var duplicateName = await CountryExistsAsync(updateDto.Name);

            if (duplicateName)
            {
                return Result.Failure(new Error(ErrorCodes.Conflict, $"Country with name {updateDto.Name} already exists."));
            }

            mapper.Map(updateDto, country);

            context.Countries.Update(country);

            await context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure();
        }
    }

    public async Task<Result> DeleteCountryAsync(int id)
    {
        try
        {
            var country = await context.Countries.FindAsync(id);

            if (country is null)
            {
                return Result.NotFound(new Error(ErrorCodes.NotFound, $"Country with ID {id} was not found."));
            }

            context.Countries.Remove(country);

            await context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure();
        }
    }

    public async Task<bool> CountryExistsAsync(int id)
    {
        return await context.Countries.AnyAsync(c => c.Id == id);
    }
    public async Task<bool> CountryExistsAsync(string name)
    {
        return await context.Countries.AnyAsync(c => c.Name.ToLower().Trim() == name.ToLower().Trim());
    }
}

