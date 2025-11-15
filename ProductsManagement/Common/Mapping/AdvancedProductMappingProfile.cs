using System;
using AutoMapper;
using ProductsManagement.Features.Products;
using ProductsManagement.Features.Products.DTOs;

namespace ProductsManagement.Common.Mapping
{
    public class AdvancedProductMappingProfile : Profile
    {
        public AdvancedProductMappingProfile()
        {
            // CreateProductProfileRequest -> Product
            CreateMap<CreateProductProfileRequest, Product>()
                .ForMember(d => d.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(d => d.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.IsAvailable, opt => opt.MapFrom(s => s.StockQuantity > 0))
                .ForMember(d => d.UpdatedAt, opt => opt.Ignore());

            // Product -> ProductProfileDto
            CreateMap<Product, ProductProfileDto>()
                // Category display name (Electronics & Technology etc.)
                .ForMember(d => d.CategoryDisplayName,
                    opt => opt.MapFrom<CategoryDisplayResolver>())

                // Conditional price (10% discount pentru Home)
                .ForMember(d => d.Price,
                    opt => opt.MapFrom(src =>
                        src.Category == ProductCategory.Home
                            ? src.Price * 0.9m
                            : src.Price))

                // FormattedPrice ca monedă
                .ForMember(d => d.FormattedPrice,
                    opt => opt.MapFrom<PriceFormatterResolver>())

                // Conditional ImageUrl (null pentru Home)
                .ForMember(d => d.ImageUrl,
                    opt => opt.MapFrom(src =>
                        src.Category == ProductCategory.Home
                            ? null
                            : src.ImageUrl))

                // Product age (New Release / X months / X years / Classic)
                .ForMember(d => d.ProductAge,
                    opt => opt.MapFrom<ProductAgeResolver>())

                // Brand initials
                .ForMember(d => d.BrandInitials,
                    opt => opt.MapFrom<BrandInitialsResolver>())

                // Availability status din IsAvailable + StockQuantity
                .ForMember(d => d.AvailabilityStatus,
                    opt => opt.MapFrom<AvailabilityStatusResolver>());
        }

        // ================== Custom Value Resolvers ==================

        // CategoryDisplayResolver
        private class CategoryDisplayResolver : IValueResolver<Product, ProductProfileDto, string>
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

        // PriceFormatterResolver
        private class PriceFormatterResolver : IValueResolver<Product, ProductProfileDto, string>
        {
            public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
            {
                var effectivePrice = source.Category == ProductCategory.Home
                    ? source.Price * 0.9m
                    : source.Price;

                return effectivePrice.ToString("C2");
            }
        }

        // ProductAgeResolver
        private class ProductAgeResolver : IValueResolver<Product, ProductProfileDto, string>
        {
            public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
            {
                var today = DateTime.UtcNow.Date;
                var release = source.ReleaseDate.Date;

                if (release > today)
                {
                    // Defensive: produs cu data din viitor
                    return "Releases in the future";
                }

                var days = (today - release).TotalDays;

                if (days < 30)
                {
                    return "New Release";
                }

                if (days < 365)
                {
                    var months = (int)(days / 30);
                    if (months < 1) months = 1;
                    return $"{months} months old";
                }

                if (days == 1825) // exact 5 ani
                {
                    return "Classic";
                }

                if (days < 1825)
                {
                    var years = (int)(days / 365);
                    if (years < 1) years = 1;
                    return $"{years} years old";
                }

                // > 5 ani – tot Classic
                return "Classic";
            }
        }

        // BrandInitialsResolver
        private class BrandInitialsResolver : IValueResolver<Product, ProductProfileDto, string>
        {
            public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
            {
                if (string.IsNullOrWhiteSpace(source.Brand))
                {
                    return "?";
                }

                var parts = source.Brand
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (parts.Length == 1)
                {
                    var word = parts[0];
                    return string.IsNullOrEmpty(word)
                        ? "?"
                        : char.ToUpperInvariant(word[0]).ToString();
                }

                var first = parts[0];
                var last = parts[^1];

                var firstInitial = !string.IsNullOrEmpty(first)
                    ? char.ToUpperInvariant(first[0])
                    : '?';

                var lastInitial = !string.IsNullOrEmpty(last)
                    ? char.ToUpperInvariant(last[0])
                    : '?';

                return $"{firstInitial}{lastInitial}";
            }
        }

        // AvailabilityStatusResolver
        private class AvailabilityStatusResolver : IValueResolver<Product, ProductProfileDto, string>
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
    }
}
