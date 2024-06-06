using Northwind.EntityModels;
using Alex.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(defaultScheme: "Bearer")
    .AddJwtBearer();

builder.Services.AddCustomRateLimiting(builder.Configuration);
builder.Services.AddCustomCors();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddNorthwindContext();
builder.Services.AddCustomHttpLogging();

var app = builder.Build();

app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseHttpLogging();

await app.UseCustomClientRateLimiting();

// app.UseCors(policyName: "Northwind.Mvc.Policy");
app.UseCors();

app.MapGets()
    .MapPosts()
    .MapPuts()
    .MapDeletes();

app.Run();