using HotelListing.API.Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.API.Domain.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole
            {
                Id = "c97aba77-7558-4a15-98e3-b56e3da45128",
                Name = "Administrator",
                NormalizedName = RoleNames.Administrator.ToUpper()
            },
            new IdentityRole
            {
                Id = "9679a22d-1829-4332-9c91-f7ad9bb21bc7",
                Name = "User",
                NormalizedName = RoleNames.User.ToUpper()
            },
            new IdentityRole
            {
                Id = "9389a22d-1867-4332-9c91-f7ad9bb21bc7",
                Name = "Hotel Admin",
                NormalizedName = RoleNames.HotelAdmin.ToUpper()
            }
        );
    }
}