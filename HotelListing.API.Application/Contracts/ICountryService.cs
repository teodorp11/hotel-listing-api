using HotelListing.API.Common.Models.Filtering;
using HotelListing.API.Common.Models.Paging;
using HotelListing.API.Common.Results;
using HotelListing.API.DTOs.Country;
using HotelListing.API.DTOs.Hotel;
using Microsoft.AspNetCore.JsonPatch;

namespace HotelListing.API.Contracts;

public interface ICountryService
{
    Task<bool> CountryExistsAsync(int id);
    Task<bool> CountryExistsAsync(string name);
    Task<Result<GetCountryDto>> CreateCountryAsync(CreateCountryDto createDto);
    Task<Result> DeleteCountryAsync(int id);
    Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync(CountryFilterParameters countryFilterParameters);
    Task<Result<GetCountryDto>> GetCountryAsync(int id);
    Task<Result> UpdateCountryAsync(int id, UpdateCountryDto updateDto);
    Task<Result<GetCountryHotelsDto>> GetCountryHotelsAsync(int countryId, PaginationParameters paginationParameters, CountryFilterParameters countryFilterParameters);
    Task<Result> PatchCountryAsync(int id, JsonPatchDocument<UpdateCountryDto> patchDocument);
}