using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

// for appsettings
using static System.Environment; // CurrentDirectory 
using static System.IO.Path; // Combine
using Microsoft.Extensions.Configuration;

namespace Northwind.EntityModels;

public partial class NorthwindContext : DbContext
{
    private static readonly SetLastRefreshedInterceptor setLastRefreshedInterceptor = new();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!optionsBuilder.IsConfigured){
                        SqlConnectionStringBuilder builder = new();

            builder.DataSource = "tcp:127.0.0.1,1433";
            builder.InitialCatalog = "Northwind";
            builder.TrustServerCertificate = true;
            builder.MultipleActiveResultSets = true;

            builder.ConnectTimeout = 3; // fail faster. Default is 15

            builder.IntegratedSecurity = false; // for windows integrated auth

            // storing UserID and Password in appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(Combine(CurrentDirectory, "appsettings.json"), optional: false, reloadOnChange: true)
                .Build();

            builder.UserID = configuration.GetConnectionString("UserID");
            builder.Password  = configuration.GetConnectionString("Password");

            optionsBuilder.UseSqlServer(builder.ConnectionString);
        }
        optionsBuilder.AddInterceptors(setLastRefreshedInterceptor);
    }
}
