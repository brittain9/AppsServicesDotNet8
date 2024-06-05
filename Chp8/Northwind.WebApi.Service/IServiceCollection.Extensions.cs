using AspNetCoreRateLimit;
using Microsoft.AspNetCore.HttpLogging;

namespace Alex.Extensions;
public static class IServiceCollectionExtensions
{

    public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services, ConfigurationManager configurationManager){
        services.AddMemoryCache();
        services.AddInMemoryRateLimiting();

        // Load default rate limit options from appsettings.json.
        services.Configure<ClientRateLimitOptions>(configurationManager.GetSection("ClientRateLimiting"));

        services.Configure<ClientRateLimitOptions>(configurationManager.GetSection("ClientRateLimitPolicies"));

        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        return services;
    }

    public static IServiceCollection AddCustomHttpLogging(this IServiceCollection services){
        services.AddHttpLogging(options =>{
            options.RequestHeaders.Add("Origin"); // Add the origin header so it will not be redirected

            // Add the rate limiting headers so they will not be redacted.
            options.RequestHeaders.Add("X-Client-Id");
            options.ResponseHeaders.Add("Retry-After");

            options.LoggingFields = HttpLoggingFields.All; // By default, the response body is not included
        });

        return services;
    }
}
