using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Reflection;

namespace HotelListing.API.Data;

public class HotelListingDbContext(DbContextOptions<HotelListingDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Country> Countries { get; set; }
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        // This stops the "PendingModelChangesWarning" from crashing your Update-Database command
        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApiKey>(b =>
        {
            b.HasIndex(k => k.Key).IsUnique();
        });

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
