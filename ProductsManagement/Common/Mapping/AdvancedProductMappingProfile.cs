using AutoMapper;
using ProductsManagement.Features.Products;
using ProductsManagement.Features.Products.DTOs;

namespace ProductsManagement.Common.Mapping;

/// <summary>
/// Advanced AutoMapper profile for products, including custom value
/// resolvers and conditional mappings.
/// </summary>
public class AdvancedProductMappingProfile : Profile
{
    public AdvancedProductMappingProfile()
    {
        // CreateProductProfileRequest -> Product
        CreateMap<CreateProductProfileRequest, Product>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.StockQuantity));

        // Product -> ProductProfileDto with custom resolvers and conditional mappings
        CreateMap<Product, ProductProfileDto>()
            .ForMember(dest => dest.CategoryDisplayName, opt => opt.MapFrom<CategoryDisplayResolver>())
            .ForMember(dest => dest.FormattedPrice, opt => opt.MapFrom<PriceFormatterResolver>())
            .ForMember(dest => dest.ProductAge, opt => opt.MapFrom<ProductAgeResolver>())
            .ForMember(dest => dest.BrandInitials, opt => opt.MapFrom<BrandInitialsResolver>())
            .ForMember(dest => dest.AvailabilityStatus, opt => opt.MapFrom<AvailabilityStatusResolver>())
            // Conditional ImageUrl mapping
            .ForMember(dest => dest.ImageUrl, opt =>
            {
                opt.PreCondition(src => src.Category != ProductCategory.Home);
                opt.MapFrom(src => src.ImageUrl);
            })
            // Conditional price mapping with discount for Home category
            .ForMember(dest => dest.Price, opt =>
            {
                opt.MapFrom(src => src.Category == ProductCategory.Home
                    ? src.Price * 0.9m
                    : src.Price);
            });
    }
}

/// <summary>
/// Resolves the display name for a product category.
/// </summary>
public class CategoryDisplayResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        return source.Category switch
        {
            ProductCategory.Electronics => "Electronics & Technology",
            ProductCategory.Clothing => "Clothing & Fashion",
            ProductCategory.Books => "Books & Media",
            ProductCategory.Home => "Home & Garden",
            _ => "Uncategorized"
        };
    }
}

/// <summary>
/// Formats the effective product price as currency.
/// </summary>
public class PriceFormatterResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        var effectivePrice = source.Category == ProductCategory.Home
            ? source.Price * 0.9m
            : source.Price;

        return effectivePrice.ToString("C2");
    }
}

/// <summary>
/// Builds a human-readable age for a product based on its release date.
/// </summary>
public class ProductAgeResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        var today = DateTime.UtcNow.Date;
        var releaseDate = source.ReleaseDate.Date;

        if (releaseDate > today)
        {
            return "Releases in the future";
        }

        var days = (today - releaseDate).TotalDays;

        if (days < 30)
        {
            return "New Release";
        }

        if (days < 365)
        {
            var months = (int)(days / 30);
            return months == 1 ? "1 month old" : $"{months} months old";
        }

        if (Math.Abs(days - 1825) < 0.5) // exactly 5 years
        {
            return "Classic";
        }

        if (days < 1825)
        {
            var years = (int)(days / 365);
            return years == 1 ? "1 year old" : $"{years} years old";
        }

        var overYears = (int)(days / 365);
        return overYears == 1 ? "1 year old" : $"{overYears} years old";
    }
}

/// <summary>
/// Resolves brand initials from the product brand name.
/// </summary>
public class BrandInitialsResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        if (string.IsNullOrWhiteSpace(source.Brand))
        {
            return "?";
        }

        var parts = source.Brand
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length >= 2)
        {
            return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}";
        }

        var word = parts[0];
        return char.ToUpperInvariant(word[0]).ToString();
    }
}

public class AvailabilityStatusResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        if (!source.IsAvailable)
        {
            return "Out of Stock";
        }

        if (source.StockQuantity <= 0)
        {
            return "Unavailable";
        }

        if (source.StockQuantity == 1)
        {
            return "Last Item";
        }

        if (source.StockQuantity <= 5)
        {
            return "Limited Stock";
        }

        return "In Stock";
    }
}
