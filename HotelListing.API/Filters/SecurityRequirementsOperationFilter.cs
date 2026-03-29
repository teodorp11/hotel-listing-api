using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HotelListing.API.Filters;

public class SecurityRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var authAttributes = context.MethodInfo.DeclaringType?
            .GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<AuthorizeAttribute>();

        if (authAttributes?.Any() == true)
        {
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

            var securityRequirements = new List<OpenApiSecurityRequirement>();

            // Check for API Key authentication
            if (context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .Any(attr => attr.GetType().Name.Contains("ApiKey")) == true)
            {
                securityRequirements.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("ApiKey", context.Document)] = []
                });
            }

            // Check for Basic authentication
            if (context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .Any(attr => attr.GetType().Name.Contains("Basic")) == true)
            {
                securityRequirements.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Basic", context.Document)] = []
                });
            }

            // Fallback to Bearer Token if [Authorize] is used but no specific scheme is mentioned
            if (!securityRequirements.Any())
            {
                securityRequirements.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
                });
            }

            if (securityRequirements.Any())
            {
                operation.Security = securityRequirements;
            }
        }
    }
}