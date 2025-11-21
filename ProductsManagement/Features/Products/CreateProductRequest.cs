using ProductsManagement.Features.Products;

namespace ProductsManagement.Features.Products.DTOs;

public record CreateProductRequest(
    string Name,
    string Brand,
    string SKU,
    ProductCategory Category,
    decimal Price,
    DateTime ReleaseDate,
    string? ImageUrl,
    int StockQuantity
);