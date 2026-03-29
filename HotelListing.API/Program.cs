using Asp.Versioning;
using HealthChecks.UI.Client;
using HotelListing.API.Common.Constants;
using HotelListing.API.Common.Models.Config;
using HotelListing.API.Contracts;
using HotelListing.API.Domain;
using HotelListing.API.Handlers;
using HotelListing.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Filters;
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

    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy("Application is running"),
            tags: ["api"])
        .AddDbContextCheck<HotelListingDbContext>(
            name: "database",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["db", "sql"]);

    // Not compatible with EF Core 10
    //builder.Services.AddHealthChecksUI(setup =>
    //{
    //    setup.SetEvaluationTimeInSeconds(10);
    //    setup.MaximumHistoryEntriesPerEndpoint(50);
    //    setup.AddHealthCheckEndpoint("HotelListing API", "/healthz");
    //})
    //.AddInMemoryStorage();

    builder.Services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        // API Information
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Hotel Listing API",
            Description = "API for managing hotels, countries, and bookings",
            Contact = new OpenApiContact
            {
                Name = "Support Team",
                Email = "support@hotellisting.com"
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        });

        options.SwaggerDoc("v2", new OpenApiInfo
        {
            Version = "v2",
            Title = "Hotel Listing API V2",
            Description = "Version 2 of the Hotel Listing API with enhanced features"
        });

        // Include XML comments
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }

        // Enable annotations
        options.EnableAnnotations();

        // Security Definitions
        // JWT Bearer Authentication
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        });

        // API Key Authentication
        options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
            Description = "API Key needed to access the endpoints. X-Api-Key: {API Key}",
            In = ParameterLocation.Header,
            Name = "X-Api-Key",
            Type = SecuritySchemeType.ApiKey
        });

        // Basic Authentication
        options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
        {
            Description = "Basic Authentication header",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "basic"
        });

        // Add security requirements
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });


        // Add operation filters for examples
        options.ExampleFilters();

        // Custom operation filter for handling multiple auth schemes
        options.OperationFilter<HotelListing.API.Filters.SecurityRequirementsOperationFilter>();

        // Order actions by method
        options.OrderActionsBy(apiDesc => $"{apiDesc.RelativePath}_{apiDesc.HttpMethod}");
    });

    builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

    Log.Information(">>> STAGE 3: Building App");

    var app = builder.Build();

    // REGISTER LIFETIME EVENTS
    app.Lifetime.ApplicationStarted.Register(() => Log.Information("SUCCESS: API is now listening on configured ports."));
    app.Lifetime.ApplicationStopping.Register(() => Log.Information("ADVISORY: API is receiving a stop signal."));

    Log.Information(">>> STAGE 4: Configuring Middleware Pipeline");

    app.UseExceptionHandler("/error");

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";

        options.GetLevel = (httpContext, elapsed, ex) => ex != null
        ? LogEventLevel.Error
        : httpContext.Response.StatusCode >= 500
            ? LogEventLevel.Error
            : httpContext.Response.StatusCode >= 400
                ? LogEventLevel.Warning
                : LogEventLevel.Information;

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("UserName", httpContext.User?.Identity?.Name ?? "anonymous");
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value ?? "unknown");
            }
        };
    });

    app.UseHttpsRedirection();

    app.MapHealthChecks("/healthz", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapHealthChecks("/healthz/live", new HealthCheckOptions
    {
        Predicate = _ => false
    });

    app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("db")
    });

    // Not compatible with EF Core 10
    //app.MapHealthChecksUI(options =>
    //{
    //    options.UIPath = "/healthchecks-ui";
    //    options.ApiPath = "/healthchecks-api";
    //});

    app.UseRateLimiter();
    
    app.UseAuthentication();
    
    app.UseAuthorization();

    // Routes
    app.MapGroup("api/defaultauth").MapIdentityApi<ApplicationUser>();
    
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel Listing API V1");
            options.SwaggerEndpoint("/swagger/v2/swagger.json", "Hotel Listing API V2");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "Hotel Listing API Documentation";
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();
            options.EnableValidator();
        });
    }
    
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