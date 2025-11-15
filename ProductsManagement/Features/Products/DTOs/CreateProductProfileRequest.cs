using System;
using MediatR;

namespace ProductsManagement.Features.Products.DTOs
{
    // Request-ul MediatR care va fi procesat de CreateProductHandler
    public class CreateProductProfileRequest : IRequest<ProductProfileDto>
    {
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public ProductCategory Category { get; set; }
        public decimal Price { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string? ImageUrl { get; set; }
        public int StockQuantity { get; set; } = 1;
    }
}
