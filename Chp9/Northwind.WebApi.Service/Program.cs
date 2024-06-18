using Microsoft.Extensions.Caching.Memory;  // Add this import

using Northwind.EntityModels; // To use the AddNorthwindContext method

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddNorthwindContext();
builder.Services.AddControllers();

builder.Services.AddSingleton<IMemoryCache>(new MemoryCache(
    new MemoryCacheOptions
    {
        TrackStatistics = true,
        SizeLimit = 50 // Products. 
    }));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
