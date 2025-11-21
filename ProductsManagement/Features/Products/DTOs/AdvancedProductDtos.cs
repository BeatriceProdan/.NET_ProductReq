using System.ComponentModel.DataAnnotations;
using ProductsManagement.Features.Products;
using ProductsManagement.Validators.Attributes;

namespace ProductsManagement.Features.Products.DTOs;

public class ProductProfileDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public string CategoryDisplayName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string FormattedPrice { get; set; } = string.Empty;

    public DateTime ReleaseDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; }

    public int StockQuantity { get; set; }

    public string ProductAge { get; set; } = string.Empty;

    public string BrandInitials { get; set; } = string.Empty;

    public string AvailabilityStatus { get; set; } = string.Empty;
}

public class CreateProductProfileRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [ValidSKU]
    public string SKU { get; set; } = string.Empty;

    [ProductCategory(ProductCategory.Electronics, ProductCategory.Clothing, ProductCategory.Books, ProductCategory.Home)]
    public ProductCategory Category { get; set; }

    [PriceRange(0.01, 9999.99)]
    public decimal Price { get; set; }

    public DateTime ReleaseDate { get; set; }

    public string? ImageUrl { get; set; }

    public int StockQuantity { get; set; } = 1;
}
