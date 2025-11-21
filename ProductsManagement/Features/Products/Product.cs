namespace ProductsManagement.Features.Products;

public class Product
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public ProductCategory Category { get; set; }

    public decimal Price { get; set; }

    public DateTime ReleaseDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? ImageUrl { get; set; }

    private int _stockQuantity = 0;

    public int StockQuantity
    {
        get => _stockQuantity;
        set
        {
            _stockQuantity = value;
            IsAvailable = _stockQuantity > 0;
        }
    }
    
    public bool IsAvailable { get; private set; }
    
    public void UpdateAvailability()
    {
        IsAvailable = StockQuantity > 0;
    }

    
    public Product()
    {
        StockQuantity = 0;
    }
}
