using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.API.Data.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.HasIndex(k => k.Key).IsUnique();

        builder.HasData(
            new ApiKey
            {
                Id = 1,
                AppName = "app",
                CreatedAtUtc = new DateTime(2026, 01, 01),
                Key = "dXNlcjZAbG9jYWxob3N0LmNvbTpQQHNzd29yZDE="
            }
        );
    }
}