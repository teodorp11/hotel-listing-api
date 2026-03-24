using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.API.Common.Constants;
using HotelListing.API.Common.Models.Extensions;
using HotelListing.API.Common.Models.Filtering;
using HotelListing.API.Common.Models.Paging;
using HotelListing.API.Common.Results;
using HotelListing.API.Contracts;
using HotelListing.API.Domain;
using HotelListing.API.DTOs.Country;
using HotelListing.API.DTOs.Hotel;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Services;

public class CountryService(HotelListingDbContext context, IMapper mapper) : ICountryService
{
    public async Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync(CountryFilterParameters countryFilterParameters)
    {
        var query = context.Countries.AsQueryable();

        if (!string.IsNullOrWhiteSpace(countryFilterParameters?.Search))
        {
            var term = countryFilterParameters.Search.Trim();
            
            query = query.Where(q => EF.Functions.Like(q.Name, $"%{term}%")
            || EF.Functions.Like(q.ShortName, $"%{term}%"));
        }

        var countries = await query
            .AsNoTracking()
            .ProjectTo<GetCountriesDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<IEnumerable<GetCountriesDto>>.Success(countries);
    }
    public async Task<Result<GetCountryDto>> GetCountryAsync(int id)
    {
        var country = await context.Countries
            .AsNoTracking()
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
    public async Task<Result<GetCountryHotelsDto>> GetCountryHotelsAsync(int countryId, PaginationParameters paginationParameters, CountryFilterParameters countryFilterParameters)
    {
        var exists = await CountryExistsAsync(countryId);
        if (!exists)
        {
            return Result<GetCountryHotelsDto>.Failure(new Error(ErrorCodes.NotFound, $"Country '{countryId}' was not found."));
        }

        var countryName = await context.Countries
            .Where(q => q.Id == countryId)
            .Select(q => q.Name)
            .SingleAsync();

        var hotelsQuery = context.Hotels
            .Where(h => h.CountryId == countryId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(countryFilterParameters.Search))
        {
            var term = countryFilterParameters.Search.Trim();
            hotelsQuery = hotelsQuery.Where(h => EF.Functions.Like(h.Name, $"%{term}%"));
        }

        hotelsQuery = (countryFilterParameters.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "name" => countryFilterParameters.SortDescending ? hotelsQuery.OrderByDescending(h => h.Name) : hotelsQuery.OrderBy(h => h.Name),
            "rating" => countryFilterParameters.SortDescending ? hotelsQuery.OrderByDescending(h => h.Rating) : hotelsQuery.OrderBy(h => h.Rating),
            _ => hotelsQuery.OrderBy(h => h.Name)
        };

        var pagedHotels = await hotelsQuery
            .AsNoTracking()
            .ProjectTo<GetHotelSlimDto>(mapper.ConfigurationProvider)
            .ToPagedResultAsync(paginationParameters);

        var result = new GetCountryHotelsDto
        {
            Id = countryId,
            Name = countryName,
            Hotels = pagedHotels
        };

        return Result<GetCountryHotelsDto>.Success(result);
    }

    public async Task<bool> CountryExistsAsync(int id)
    {
        return await context.Countries
            .AsNoTracking()
            .AnyAsync(c => c.Id == id);
    }
    public async Task<bool> CountryExistsAsync(string name)
    {
        return await context.Countries
            .AsNoTracking()
            .AnyAsync(c => c.Name.ToLower().Trim() == name.ToLower().Trim());
    }

    public async Task<Result> PatchCountryAsync(int id, JsonPatchDocument<UpdateCountryDto> patchDocument)
    {
        var country = await context.Countries.FindAsync(id);
        
        if (country is null)
        {
            return Result.NotFound(new Error(ErrorCodes.NotFound, $"Country '{id}' was not found."));
        }

        var countryDto = mapper.Map<UpdateCountryDto>(country);

        patchDocument.ApplyTo(countryDto);

        if (countryDto.Id != id)
        {
            return Result.BadRequest(new Error(ErrorCodes.Validation, "Cannot modify the Id field."));
        }

        var normalizedName = countryDto.Name.ToLower().Trim();
        
        var duplicateExists = await context.Countries
            .AnyAsync(c => c.Name.ToLower().Trim() == normalizedName 
            && c.Id != id);

        if (duplicateExists)
        {
            return Result.Failure(new Error(ErrorCodes.Conflict,
                $"Country with name '{countryDto.Name}' already exists."));
        }

        mapper.Map(countryDto, country);
        
        context.Entry(country).State = EntityState.Modified;
        
        await context.SaveChangesAsync();

        return Result.Success();
    }
}

