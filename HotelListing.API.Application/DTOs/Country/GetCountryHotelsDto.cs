using HotelListing.API.Common.Models.Paging;
using HotelListing.API.DTOs.Hotel;

namespace HotelListing.API.DTOs.Country;

public class GetCountryHotelsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PagedResult<GetHotelSlimDto> Hotels { get; set; } = new();
}