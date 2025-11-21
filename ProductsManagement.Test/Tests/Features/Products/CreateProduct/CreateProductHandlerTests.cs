using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ProductsManagement.Common.Logging;
using ProductsManagement.Common.Mapping;
using ProductsManagement.Features.Products;
using ProductsManagement.Features.Products.DTOs;
using ProductsManagement.Persistence;
using ProductsManagement.Test.Common;
using ProductsManagement.Validators;
using Xunit;

namespace ProductsManagement.Test.Features.Products.CreateProduct;

public class CreateProductHandlerTests : IDisposable
{
    private readonly ProductsManagementContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly TestLogger<CreateProductHandler> _logger;
    private readonly CreateProductProfileValidator _validator;

    public CreateProductHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ProductsManagementContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ProductsManagementContext(options);

        var loggerFactory = LoggerFactory.Create(builder => { });

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProductMappingProfile>();
            cfg.AddProfile<AdvancedProductMappingProfile>();
        }, loggerFactory);

        _mapper = config.CreateMapper();
        _cache = new MemoryCache(new MemoryCacheOptions());

        _logger = new TestLogger<CreateProductHandler>();
        var validatorLogger = new TestLogger<CreateProductProfileValidator>();

        _validator = new CreateProductProfileValidator(_dbContext, validatorLogger);
    }

    
    // TEST 1 — VALID ELECTRONICS REQUEST
    [Fact]
    public async Task Given_ValidElectronicsProduct_When_HandleIsCalled_Then_ProductIsCreatedWithCorrectMappings()
    {
        var handler = new CreateProductHandler(_dbContext, _mapper, _validator, _logger, _cache);

        var request = new CreateProductProfileRequest
        {
            Name = "Gaming Laptop Pro",
            Brand = "Acme Corp",
            SKU = "LAP-12345",
            Category = ProductCategory.Electronics,
            Price = 1500m,
            ReleaseDate = DateTime.UtcNow.AddMonths(-2),
            ImageUrl = "https://example.com/laptop.jpg",
            StockQuantity = 3
        };

        var result = await handler.Handle(request);

        result.Should().NotBeNull();

        var product = await _dbContext.Products
            .FirstOrDefaultAsync(x => x.SKU == request.SKU);

        product.Should().NotBeNull();

        var dto = _mapper.Map<ProductProfileDto>(product!);

        dto.CategoryDisplayName.Should().Be("Electronics & Technology");
        dto.BrandInitials.Should().Be("AC");
        dto.ProductAge.Should().NotBeNullOrWhiteSpace();
        dto.FormattedPrice.Should().NotBeNullOrWhiteSpace();
        dto.AvailabilityStatus.Should().Be("Limited Stock");

        _logger.Entries.Count(e => e.EventId.Id == LogEvents.ProductCreationStarted)
            .Should().Be(1);

        _logger.Entries.Count(e => e.EventId.Id == LogEvents.ProductCreationCompleted)
            .Should().Be(2);
    }

   
    // TEST 2 — DUPLICATE SKU
  
    [Fact]
    public async Task Given_DuplicateSKU_When_HandleIsCalled_Then_ValidationExceptionIsThrownAndLogged()
    {
        var existing = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Prod",
            Brand = "X",
            SKU = "SKU-1",
            Category = ProductCategory.Books,
            Price = 10m,
            ReleaseDate = DateTime.UtcNow.AddDays(-5),
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            StockQuantity = 5
        };

        _dbContext.Products.Add(existing);
        await _dbContext.SaveChangesAsync();

        var handler = new CreateProductHandler(_dbContext, _mapper, _validator, _logger, _cache);

        var request = new CreateProductProfileRequest
        {
            Name = "Another",
            Brand = "Brand",
            SKU = "SKU-1",
            Category = ProductCategory.Books,
            Price = 20m,
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            StockQuantity = 2
        };

        var act = async () => await handler.Handle(request);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();

        _logger.Entries.Any(e => e.EventId.Id == LogEvents.ProductValidationFailed)
            .Should().BeTrue();
    }

   
    // TEST 3 — HOME CATEGORY DISCOUNT
   
    [Fact]
    public async Task Given_HomeCategoryProduct_When_HandleIsCalled_Then_DiscountAndConditionalMappingAreApplied()
    {
        var handler = new CreateProductHandler(_dbContext, _mapper, _validator, _logger, _cache);

        var request = new CreateProductProfileRequest
        {
            Name = "Decorative Pillow",
            Brand = "Home Brand",
            SKU = "HOME-999",
            Category = ProductCategory.Home,
            Price = 100m,
            ReleaseDate = DateTime.UtcNow.AddMonths(-3),
            ImageUrl = "https://example.com/pillow.jpg",
            StockQuantity = 10
        };

        await handler.Handle(request);

        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.SKU == request.SKU);

        product.Should().NotBeNull();

        var dto = _mapper.Map<ProductProfileDto>(product!);

        dto.CategoryDisplayName.Should().Be("Home & Garden");
        dto.Price.Should().Be(90m);     // discount 10%
        dto.ImageUrl.Should().BeNull(); // imagine ascunsă

        _logger.Entries.Count(e => e.EventId.Id == LogEvents.CacheOperationPerformed)
            .Should().Be(1);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _cache.Dispose();
    }
}
