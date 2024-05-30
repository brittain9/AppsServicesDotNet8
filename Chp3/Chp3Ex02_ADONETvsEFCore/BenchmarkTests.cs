using BenchmarkDotNet.Attributes;
using System.Data;

using Microsoft.Data.SqlClient;
using Northwind.EntityModels;

// for appsettings
using static System.Environment; // CurrentDirectory 
using static System.IO.Path; // Combine
using Microsoft.Extensions.Configuration;
using BenchmarkDotNet.Toolchains.Roslyn; // ConfigurationBuilder

public class ADONETvsEFCore
{
    private string ConString;

    public ADONETvsEFCore()
    {   
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

        ConString = builder.ConnectionString;
    }


    // benchmarks to retreive every product from database
    [Benchmark(Baseline = true)]
    public List<Product> ADONETTest(){
        var products = new List<Product>();

        using (SqlConnection sqlConnection = new SqlConnection(ConString))
        {
            sqlConnection.Open();

            using (SqlCommand cmd = sqlConnection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM Products";

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var product = new Product
                        {
                            ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                            ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                            SupplierId = reader.IsDBNull(reader.GetOrdinal("SupplierId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("SupplierId")),
                            CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("CategoryId")),
                            QuantityPerUnit = reader.IsDBNull(reader.GetOrdinal("QuantityPerUnit")) ? null : reader.GetString(reader.GetOrdinal("QuantityPerUnit")),
                            UnitPrice = reader.IsDBNull(reader.GetOrdinal("UnitPrice")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                            UnitsInStock = reader.IsDBNull(reader.GetOrdinal("UnitsInStock")) ? (short?)null : reader.GetInt16(reader.GetOrdinal("UnitsInStock")),
                            UnitsOnOrder = reader.IsDBNull(reader.GetOrdinal("UnitsOnOrder")) ? (short?)null : reader.GetInt16(reader.GetOrdinal("UnitsOnOrder")),
                            ReorderLevel = reader.IsDBNull(reader.GetOrdinal("ReorderLevel")) ? (short?)null : reader.GetInt16(reader.GetOrdinal("ReorderLevel")),
                            Discontinued = reader.GetBoolean(reader.GetOrdinal("Discontinued"))
                        };

                        products.Add(product);
                    }
                }
            }
        }
        return products.ToList();
    }

    [Benchmark]
    public List<Product>  EFCoreTest(){

        using (NorthwindContext db = new())
        {
            return db.Products.ToList();
        } 
    } 
}
