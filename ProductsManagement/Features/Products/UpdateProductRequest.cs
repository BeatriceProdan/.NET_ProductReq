namespace ProductsManagement.Features.Products;

public record UpdateProductRequest(
    Guid Id,
    string Name,
    string Brand,
    string SKU,
    ProductCategory Category,
    decimal Price,
    DateTime ReleaseDate,
    string? ImageUrl,
    int StockQuantity);