﻿using Microsoft.Data.SqlClient;
using System.Data;

using System.Text.Json; // To use Utf8JsonWriter, JsonSerializer
using static System.Environment;
using static System.IO.Path;

using Northwind.Models;
using Dapper;

ConfigureConsole();

#region Set up the connection string builder

SqlConnectionStringBuilder builder = new(){
    InitialCatalog = "Northwind",
    MultipleActiveResultSets = true,
    Encrypt = true,
    TrustServerCertificate = true,
    ConnectTimeout = 10 // default is 30
};

WriteLine("Connect to:");
WriteLine("  1 - SQL Server on local machine");
WriteLine("  2 - Azure SQL Database");
WriteLine("  3 - Azure SQL Edge");
WriteLine();
Write("Press a key: ");

ConsoleKey key = ReadKey().Key;
WriteLine(); WriteLine();

switch (key){
    case ConsoleKey.D1 or ConsoleKey.NumPad1:
        builder.DataSource = ".";
        break;
    case ConsoleKey.D2 or ConsoleKey.NumPad2:
        builder.DataSource = "database name"; // use database name here
        break;
    case ConsoleKey.D3 or ConsoleKey.NumPad3:
        builder.DataSource = "tcp:127.0.0.1,1433";
        break;
    default:
        WriteLine("No datasource selected");
        return;
}

WriteLine("Authenticate using:");
WriteLine("  1 - Windows Integrated Security");
WriteLine("  2 - SQL Login, for example, sa");
WriteLine();
Write("Press a key:");

key = ReadKey().Key;
WriteLine(); WriteLine();

if (key is ConsoleKey.D1 or ConsoleKey.NumPad1){
    builder.IntegratedSecurity = true;
}else if (key is ConsoleKey.D2 or ConsoleKey.NumPad2){
    Write("Enter your SQL Server user id: ");
    string? userId = ReadLine();
    if(string.IsNullOrWhiteSpace(userId)){
        WriteLine("User ID cannot be empty or null");
        return;
    }

    builder.UserID = userId;

    Write("Enter your SQL Server password: ");
    string? password = ReadLine();
    if(string.IsNullOrWhiteSpace(password)){
        WriteLine("Password cannot be empty or null");
        return;
    }

    builder.Password = password;
    builder.PersistSecurityInfo = false;
} 
else{
    WriteLine("No authenication selected");
    return;
}

#endregion

#region Create and open the connection

SqlConnection connection = new(builder.ConnectionString);

WriteLine(connection.ConnectionString);
WriteLine();

connection.StateChange += Connection_StateChange;
connection.InfoMessage += Connection_InfoMessage;

try{
    WriteLine("Opening connection. Please wait up to {0} seconds...", builder.ConnectTimeout);
    WriteLine();
    await connection.OpenAsync();

    WriteLine($"SQL Server version: {connection.ServerVersion}");

    connection.StatisticsEnabled = true;
}
catch (SqlException ex){
    WriteLineInColor($"SQL exception: {ex.Message}", ConsoleColor.Red);
    return;
}

#endregion

WriteLine("Enter a unit price: ");
string? priceText = ReadLine();

if(!decimal.TryParse(priceText, out decimal price)){
    WriteLine("You must enter a valid unit price.");
    return;
}

SqlCommand cmd = connection.CreateCommand();

WriteLine("Execute command using:");
WriteLine("  1 - Text");
WriteLine("  2 - Stored Procedure");
WriteLine();
Write("Press a key:");

key = ReadKey().Key;
WriteLine(); WriteLine();

SqlParameter p1, p2 = new(), p3 = new();

if (key is ConsoleKey.D1 or ConsoleKey.NumPad1){
    cmd.CommandType = CommandType.Text;
    cmd.CommandText = "SELECT ProductId, ProductName, UnitPrice FROM Products"
        + " WHERE UnitPrice >= @minimumPrice";

    cmd.Parameters.AddWithValue("minimumPrice", price);
}
else if (key is ConsoleKey.D2 or ConsoleKey.NumPad2){
    cmd.CommandType = CommandType.StoredProcedure;
    cmd.CommandText = "GetExpensiveProducts2";

    p1 = new(){
        ParameterName = "price",
        SqlDbType = SqlDbType.Money,
        SqlValue = price,
    };

    p2 = new(){
        Direction = ParameterDirection.Output,
        ParameterName = "count",
        SqlDbType = SqlDbType.Int
    };

    p3 = new(){
        Direction = ParameterDirection.ReturnValue,
        ParameterName = "rv",
        SqlDbType= SqlDbType.Int
    };

    cmd.Parameters.AddRange(new[]{p1, p2, p3});
} 


SqlDataReader r = await cmd.ExecuteReaderAsync();

string horizontalLine = new string('-', 60);
WriteLine(horizontalLine);
WriteLine("| {0,5} | {1,-35} | {2,10} |",
    arg0: "Id", arg1: "Name", arg2: "Price"
);
WriteLine(horizontalLine);

// define a file path to write
string jsonPath = Combine(CurrentDirectory, "products.json");

List<Product> products = new(capacity:77);

await using (FileStream jsonStream = File.Create(jsonPath)){
    Utf8JsonWriter jsonWriter = new(jsonStream);
    jsonWriter.WriteStartArray();

    while(await r.ReadAsync()){
        Product product = new Product(){
            ProductId = await r.GetFieldValueAsync<int>("ProductId"),
            ProductName = await r.GetFieldValueAsync<string>("ProductName"),
            UnitPrice = await r.GetFieldValueAsync<decimal>("UnitPrice")
        };
        products.Add(product);

        WriteLine("| {0,5} | {1,-35} | {2,10:C} |",
        await r.GetFieldValueAsync<int>("ProductId"),
        await r.GetFieldValueAsync<string>("ProductName"),
        await r.GetFieldValueAsync<Decimal>("UnitPrice"));

        jsonWriter.WriteStartObject();

        jsonWriter.WriteNumber("productId",
            await r.GetFieldValueAsync<int>("ProductId"));
        jsonWriter.WriteString("productName",
            await r.GetFieldValueAsync<string>("ProductName"));
        jsonWriter.WriteNumber("unitPrice",
            await r.GetFieldValueAsync<decimal>("UnitPrice"));

        jsonWriter.WriteEndObject();
    }

    jsonWriter.WriteEndArray();
    jsonWriter.Flush();
    jsonStream.Close();
}


WriteLine(horizontalLine);

WriteLineInColor($"Written to: {jsonPath}", ConsoleColor.DarkGreen);
WriteLineInColor(JsonSerializer.Serialize(products), ConsoleColor.Magenta);

await r.CloseAsync();

if(key is ConsoleKey.D2 or ConsoleKey.NumPad2){
    WriteLine($"Output count: {p2.Value}");
    WriteLine($"Return value: {p3.Value}");
}

OutputStatistics(connection);

WriteLineInColor("Using Dapper", ConsoleColor.DarkGreen);
connection.ResetStatistics();

IEnumerable<Supplier> suppliers= connection.Query<Supplier>(
    sql: "SELECT * FROM Suppliers WHERE Country=@Country",
    param: new{Country = "Germany"});

foreach(Supplier supplier in suppliers)
{
    WriteLine("{0}: {1}, {2}, {3}",
    supplier.SupplierId, supplier.CompanyName, supplier.City, supplier.Country);
}

WriteLineInColor(JsonSerializer.Serialize(suppliers), ConsoleColor.Green);

IEnumerable<Product> productsFromDapper = connection.Query<Product>(
    sql: "GetExpensiveProducts2",
    param: new {price = 100M, count = 0},
    commandType: CommandType.StoredProcedure
    );

foreach (Product product in productsFromDapper)
{
    WriteLine("{0}: {1}, {2}",
    product.ProductId, product.ProductName, product.UnitPrice);
}

WriteLineInColor(JsonSerializer.Serialize(productsFromDapper), ConsoleColor.Green);

OutputStatistics(connection);

await connection.CloseAsync();