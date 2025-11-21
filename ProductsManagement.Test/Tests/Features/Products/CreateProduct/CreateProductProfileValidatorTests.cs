using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductsManagement.Features.Products;
using ProductsManagement.Features.Products.DTOs;
using ProductsManagement.Persistence;
using ProductsManagement.Test.Common;
using ProductsManagement.Validators;
using Xunit;

namespace ProductsManagement.Test.Features.Products.CreateProduct;

public class CreateProductProfileValidatorTests : IDisposable
{
    private readonly ProductsManagementContext _dbContext;
    private readonly CreateProductProfileValidator _validator;

    public CreateProductProfileValidatorTests()
    {
        var options = new DbContextOptionsBuilder<ProductsManagementContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ProductsManagementContext(options);

        var logger = new TestLogger<CreateProductProfileValidator>();
        _validator = new CreateProductProfileValidator(_dbContext, logger);
    }

    
    // VALID REQUEST
    
    [Fact]
    public async Task Given_ValidProduct_When_Validated_Then_ValidationSucceeds()
    {
        var request = new CreateProductProfileRequest
        {
            Name = "Gaming Laptop Pro", 
            Brand = "Valid Brand",
            SKU = "LAP-12345",   
            Category = ProductCategory.Electronics,  
            Price = 1500m,                    
            ReleaseDate = DateTime.UtcNow.AddMonths(-2), 
            ImageUrl = "https://example.com/product.jpg", 
            StockQuantity = 3 
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }



   
    // DUPLICATE SKU
    
    [Fact]
    public async Task Given_ExistingSku_When_Validated_Then_ValidationFailsWithSkuError()
    {
        var existing = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Existing",
            Brand = "Brand",
            SKU = "DUP-001",
            Category = ProductCategory.Books,
            Price = 50m,
            ReleaseDate = DateTime.UtcNow.AddMonths(-2),
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            StockQuantity = 3
        };

        _dbContext.Products.Add(existing);
        await _dbContext.SaveChangesAsync();

        var request = new CreateProductProfileRequest
        {
            Name = "New Product",
            Brand = "New Brand",
            SKU = "DUP-001",  // duplicat
            Category = ProductCategory.Books,
            Price = 60m,
            ReleaseDate = DateTime.UtcNow.AddMonths(-1),
            StockQuantity = 2
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SKU");
    }

   
    // INVALID PRICE
    
    [Fact]
    public async Task Given_NegativePrice_When_Validated_Then_ValidationFails()
    {
        var request = new CreateProductProfileRequest
        {
            Name = "Product",
            Brand = "Brand",
            SKU = "NEG-PRICE",
            Category = ProductCategory.Electronics,
            Price = -10m,
            ReleaseDate = DateTime.UtcNow.AddMonths(-1),
            StockQuantity = 1
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    // INVALID NAME (EMPTY)
    [Fact]
    public async Task Given_EmptyName_When_Validated_Then_ValidationFails()
    {
        var request = new CreateProductProfileRequest
        {
            Name = "",
            Brand = "Brand",
            SKU = "NO-NAME",
            Category = ProductCategory.Electronics,
            Price = 10m,
            ReleaseDate = DateTime.UtcNow.AddMonths(-1),
            StockQuantity = 1
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
