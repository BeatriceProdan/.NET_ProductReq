using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;
using ProductsManagement.Common.Mapping;
using ProductsManagement.Common.Middleware;
using ProductsManagement.Features.Products;
using ProductsManagement.Features.Products.DTOs;
using ProductsManagement.Persistence;
using ProductsManagement.Validators;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Products Management API",
        Version = "v1",
        Description = "Advanced products management API with mapping, validation, logging and telemetry."
    });
});

builder.Services.AddDbContext<ProductsManagementContext>(options =>
    options.UseSqlite("Data Source=productsmanagement.db"));

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

var mapperConfig = new MapperConfiguration(
    cfg =>
    {
        cfg.AddProfile<ProductMappingProfile>();
        cfg.AddProfile<AdvancedProductMappingProfile>();
    },
    loggerFactory
);

builder.Services.AddSingleton(mapperConfig);
builder.Services.AddSingleton<IMapper>(sp => mapperConfig.CreateMapper());


builder.Services.AddMemoryCache();

builder.Services.AddScoped<CreateProductHandler>();
builder.Services.AddScoped<GetAllProductsHandler>();
builder.Services.AddScoped<GetProductByIdHandler>();
builder.Services.AddScoped<DeleteProductHandler>();
builder.Services.AddScoped<UpdateProductHandler>();


builder.Services.AddScoped<IValidator<CreateProductProfileRequest>, CreateProductProfileValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductProfileValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductsManagementContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();


app.UseCorrelationMiddleware();
app.UseCors("DevCors");
app.UseHttpsRedirection();

app.MapGet("products", async (GetAllProductsHandler handler, CancellationToken ct) =>
        await handler.Handle(new GetAllProductsRequest(), ct))
    .WithName("GetAllProducts")
    .WithOpenApi(operation =>
    {
        operation.Summary = "Get all products";
        operation.Description = "Returns all products with their mapped profile data.";
        return operation;
    });

app.MapGet("/products/{id:guid}", async (Guid id, GetProductByIdHandler handler, CancellationToken ct) =>
        await handler.Handle(new GetProductByIdRequest(id), ct))
    .WithName("GetProductById")
    .WithOpenApi(operation =>
    {
        operation.Summary = "Get product by id";
        operation.Description = "Returns a single product by its id.";
        return operation;
    });

app.MapPost("/products", async (CreateProductProfileRequest request, CreateProductHandler handler, CancellationToken ct) =>
        await handler.Handle(request, ct))
    .WithName("CreateProduct")
    .WithOpenApi(operation =>
    {
        operation.Summary = "Create a new product";
        operation.Description = "Creates a new product with advanced validation, logging, performance tracking and mapping.";
        return operation;
    });

app.MapDelete("/products/{id:guid}", async (Guid id, DeleteProductHandler handler, CancellationToken ct) =>
        await handler.Handle(new DeleteProductRequest(id), ct))
    .WithName("DeleteProduct")
    .WithOpenApi(operation =>
    {
        operation.Summary = "Delete a product";
        operation.Description = "Deletes a product if it exists.";
        return operation;
    });

app.MapPut("/products/{id:guid}", async (Guid id, UpdateProductRequest request, UpdateProductHandler handler, CancellationToken ct) =>
    {
        var updatedRequest = request with { Id = id };
        return await handler.Handle(updatedRequest, ct);
    })
    .WithName("UpdateProduct")
    .WithOpenApi(operation =>
    {
        operation.Summary = "Update a product";
        operation.Description = "Updates an existing product's editable fields.";
        return operation;
    });


app.Run();

public partial class Program
{
}
