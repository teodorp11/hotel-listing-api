using HotelListing.API.Common.Constants;
using HotelListing.API.Common.Models.Config;
using HotelListing.API.Contracts;
using HotelListing.API.Domain;
using HotelListing.API.Handlers;
using HotelListing.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Reflection;
using System.Text;

// 1. Enable Serilog Internal Debugging to catch sink failures
Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine($"SERILOG DIAGNOSTIC: {msg}"));

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() // Set to Debug for investigation
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information(">>> STAGE 1: Initializing Builder");
    
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, shared: true)
);

    Log.Information(">>> STAGE 2: Configuring Services");
    
    var connectionString = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");

    // Explicit check for Connection String
    if (string.IsNullOrEmpty(connectionString))
    {
        Log.Warning("Connection string 'HotelListingDbConnectionString' is null or empty!");
    }

    builder.Services.AddDbContextPool<HotelListingDbContext>(options =>
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.CommandTimeout(30);
            sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
        });
        
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    }, poolSize: 128);

    builder.Services.AddControllers()
        .AddNewtonsoftJson()
        .AddJsonOptions(opt => opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

    builder.Services.AddProblemDetails();

    builder.Services.AddScoped<ICountryService, CountryService>();
    builder.Services.AddScoped<IHotelService, HotelService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IApiKeyValidatorService, ApiKeyValidatorService>();
    builder.Services.AddScoped<IBookingService, BookingService>();

    builder.Services.AddAutoMapper(config => { }, Assembly.GetExecutingAssembly());

    builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<HotelListingDbContext>();

    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();

    if (string.IsNullOrWhiteSpace(jwtSettings.Key))
    {
        Log.Fatal("FATAL: JwtSettings:Key is missing from configuration");
        
        throw new Exception("Key Missing");
    }

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero
        };
    })
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(AuthenticationDefaults.BasicScheme, _ => { })
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(AuthenticationDefaults.ApiKeyScheme, _ => { });

    builder.Services.AddAuthorization();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddOpenApi();
    builder.Services.AddMemoryCache();

    // Rate Limiter
    builder.Services.AddRateLimiter(options => {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddFixedWindowLimiter(RateLimitingConstants.FixedPolicy, opt => {
            opt.Window = TimeSpan.FromMinutes(1);
            opt.PermitLimit = 50;
        });
    });

    Log.Information(">>> STAGE 3: Building App");

    var app = builder.Build();

    // REGISTER LIFETIME EVENTS
    app.Lifetime.ApplicationStarted.Register(() => Log.Information("SUCCESS: API is now listening on configured ports."));
    app.Lifetime.ApplicationStopping.Register(() => Log.Information("ADVISORY: API is receiving a stop signal."));

    Log.Information(">>> STAGE 4: Configuring Middleware Pipeline");

    app.UseExceptionHandler("/error");
    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    // Routes
    app.MapGroup("api/defaultauth").MapIdentityApi<ApplicationUser>();
    if (app.Environment.IsDevelopment()) app.MapOpenApi();
    app.MapControllers();

    Log.Information(">>> STAGE 5: Running App...");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "CRITICAL: Application terminated unexpectedly during startup");
}
finally
{
    Log.Information(">>> FINAL: CloseAndFlush initiated");
    Log.CloseAndFlush();
}