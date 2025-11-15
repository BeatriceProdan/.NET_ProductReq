using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductsManagement.Common.Mapping;
using ProductsManagement.Common.Middleware;
using ProductsManagement.Data;
using ProductsManagement.Features.Products;
using ProductsManagement.Features.Products.DTOs;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------
// Database (InMemory)
// ---------------------------
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseInMemoryDatabase("ProductsDB"));

// ---------------------------
// AutoMapper Profiles
// ---------------------------
builder.Services.AddAutoMapper(typeof(AdvancedProductMappingProfile));

// ---------------------------
// MediatR
// ---------------------------
builder.Services.AddMediatR(typeof(CreateProductProfileRequest).Assembly);


// ---------------------------
// Memory Cache
// ---------------------------
builder.Services.AddMemoryCache();

// ---------------------------
// Logging - console
// ---------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// ---------------------------
// Middleware – Correlation ID
// ---------------------------
app.UseCorrelationMiddleware();

// ---------------------------
// Products Endpoint – minimal API
// ---------------------------
app.MapPost("/products", async (
    CreateProductProfileRequest request,
    IMediator mediator) =>
{
    var result = await mediator.Send(request);
    return Results.Ok(result);
});

app.Run();
