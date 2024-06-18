using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// for appsettings
using static System.Environment; // CurrentDirectory 
using static System.IO.Path; // Combine
using Microsoft.Extensions.Configuration;

namespace Northwind.EntityModels;

public static class NorthwindContextExtensions
{
/// <summary>
/// Adds the NorthwindContext to the specified IServiceCollection. Uses the SqlServer database provider
/// </summary>
/// <param name="services"> The server collection.</param>
/// <param name="connectionString"> Set to override the default</param>
/// <returns>An IServiceCollection that can be used to add more services.</returns>

    public static IServiceCollection AddNorthwindContext(
        this IServiceCollection services,
        string? connectionString = null
    ){
        if(connectionString == null){
            SqlConnectionStringBuilder builder = new();

            builder.DataSource = "tcp:127.0.0.1,1433";
            builder.InitialCatalog = "Northwind";
            builder.TrustServerCertificate = true;
            builder.MultipleActiveResultSets = true;

            builder.ConnectTimeout = 3; // fail faster. Default is 15

            builder.IntegratedSecurity = false; // for windows integrated auth

            builder.UserID = Environment.GetEnvironmentVariable("SQL_USER");
            builder.Password = Environment.GetEnvironmentVariable("SQL_PASSWORD");

            connectionString = builder.ConnectionString;
        }

        services.AddDbContext<NorthwindContext>(options =>{
            options.UseSqlServer(connectionString);

            // Log to console when executing EF Core commands.
            options.LogTo(Console.WriteLine, new[] {Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuting});
        },
        // Register with a transiet lifetime to avoid concurrency issues with Blazor Server projects
        contextLifetime: ServiceLifetime.Transient,
        optionsLifetime: ServiceLifetime.Transient);

        return services;
    }               
}