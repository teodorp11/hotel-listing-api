using System;
using System.Collections.Generic;
using System.Text;

namespace HotelListing.API.Common.Models.Paging;

public class PagedResult<T>
{
    public IEnumerable<T> Data { get; set; } = [];
    public PaginationMetadata Metadata { get; set; } = new();
}
