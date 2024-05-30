using Microsoft.Data.SqlClient; // SqlConnectionStringBuilder
using Microsoft.EntityFrameworkCore; // QueryString, GetConnectionString
using Northwind.Models;
// for appsettings
using static System.Environment; // CurrentDirectory 
using static System.IO.Path; // Combine
using Microsoft.Extensions.Configuration; // ConfigurationBuilder

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

DbContextOptionsBuilder<NorthwindDb> options = new();
options.UseSqlServer(builder.ConnectionString);

using (NorthwindDb db = new(options.Options)){
    Write("Enter a unit price: ");
    string? priceText = ReadLine();

    if(!decimal.TryParse(priceText, out decimal price)){
        WriteLine("You must enter a valid unit price.");
        return;
    }

    // we have to use var because we are projecting into an anon type
    var products = db.Products
        .AsNoTracking()
        .Where(p => p.UnitPrice > price)
        .Select(p => new {p.ProductId, p.ProductName, p.UnitPrice});

        WriteLine("-------------------------------------------------");
        WriteLine("| {0,5} | {1,-35} | {2,8} |", "Id", "Name", "Price");
        WriteLine("-------------------------------------------------");

        foreach(var product in products){
            WriteLine("| {0,5} | {1,-35} | {2,8} |",
                product.ProductId, product.ProductName, product.UnitPrice);
        }

        WriteLine("-------------------------------------------------");

        WriteLine(products.ToQueryString());
        WriteLine();
        WriteLine($"Provider:   {db.Database.ProviderName}");
        WriteLine($"Connection: {db.Database.GetConnectionString()}");

}