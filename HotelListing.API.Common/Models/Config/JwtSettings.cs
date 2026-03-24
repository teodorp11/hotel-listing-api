using System;
using System.Collections.Generic;
using System.Text;

namespace HotelListing.API.Common.Models.Config;

public sealed class JwtSettings
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public int DurationInMinutes { get; init; }
}