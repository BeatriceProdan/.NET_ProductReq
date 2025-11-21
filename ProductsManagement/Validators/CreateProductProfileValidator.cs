using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductsManagement.Features.Products;
using ProductsManagement.Features.Products.DTOs;
using ProductsManagement.Persistence;

namespace ProductsManagement.Validators;

public class CreateProductProfileValidator : AbstractValidator<CreateProductProfileRequest>
{
    private static readonly string[] InappropriateWords =
    [
        "banned",
        "illegal",
        "inappropriate"
    ];

    private static readonly string[] HomeRestrictedWords =
    [
        "weapon",
        "explosive"
    ];

    private static readonly string[] TechnologyKeywords =
    [
        "phone",
        "laptop",
        "tablet",
        "camera",
        "tv",
        "monitor",
        "console"
    ];

    private readonly ProductsManagementContext _context;
    private readonly ILogger<CreateProductProfileValidator> _logger;

    public CreateProductProfileValidator(ProductsManagementContext context, ILogger<CreateProductProfileValidator> logger)
    {
        _context = context;
        _logger = logger;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(1).MaximumLength(200)
            .Must(BeValidName).WithMessage("Name contains inappropriate content.")
            .MustAsync(BeUniqueName).WithMessage("A product with the same name and brand already exists.");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Brand is required.")
            .MinimumLength(2).MaximumLength(100)
            .Must(BeValidBrandName).WithMessage("Brand contains invalid characters.");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required.")
            .Must(BeValidSKU).WithMessage("SKU must be alphanumeric, 5-20 characters, and may contain hyphens.")
            .MustAsync(BeUniqueSKU).WithMessage("SKU already exists.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category is not valid.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .LessThan(10_000).WithMessage("Price must be less than 10,000.");

        RuleFor(x => x.ReleaseDate)
            .Must(date => date >= new DateTime(1900, 1, 1))
            .WithMessage("Release date cannot be before 1900.")
            .LessThanOrEqualTo(_ => DateTime.UtcNow)
            .WithMessage("Release date cannot be in the future.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.")
            .LessThanOrEqualTo(100_000).WithMessage("Stock quantity cannot exceed 100,000.");

        RuleFor(x => x.ImageUrl)
            .Must(BeValidImageUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl))
            .WithMessage("ImageUrl must be a valid HTTP/HTTPS URL and point to an image file.");

        RuleFor(x => x)
            .MustAsync(PassBusinessRules)
            .WithMessage("Product violates one or more business rules.");
    }

    private bool BeValidName(string name)
    {
        var lower = name.ToLowerInvariant();
        return !InappropriateWords.Any(w => lower.Contains(w));
    }

    private async Task<bool> BeUniqueName(CreateProductProfileRequest model, string name, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking uniqueness for product name {Name} and brand {Brand}", model.Name, model.Brand);

        return !await _context.Products
            .AnyAsync(p => p.Name == model.Name && p.Brand == model.Brand, cancellationToken);
    }

    private bool BeValidBrandName(string brand)
    {
        var regex = new Regex("^[A-Za-z0-9 .'-]+$", RegexOptions.Compiled);
        return regex.IsMatch(brand);
    }

    private bool BeValidSKU(string sku)
    {
        sku = sku.Replace(" ", string.Empty);
        var regex = new Regex("^[A-Za-z0-9-]{5,20}$", RegexOptions.Compiled);
        return regex.IsMatch(sku);
    }

    private async Task<bool> BeUniqueSKU(CreateProductProfileRequest model, string sku, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking SKU uniqueness for {SKU}", sku);
        return !await _context.Products.AnyAsync(p => p.SKU == sku, cancellationToken);
    }

    private bool BeValidImageUrl(string imageUrl)
    {
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        var path = uri.AbsolutePath.ToLowerInvariant();
        return path.EndsWith(".jpg") ||
               path.EndsWith(".jpeg") ||
               path.EndsWith(".png") ||
               path.EndsWith(".gif") ||
               path.EndsWith(".webp");
    }

    private async Task<bool> PassBusinessRules(CreateProductProfileRequest request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var todaysCount = await _context.Products
            .CountAsync(p => p.CreatedAt.Date == today, cancellationToken);
        if (todaysCount >= 500)
        {
            _logger.LogWarning("Daily product addition limit reached: {Count}", todaysCount);
            return false;
        }

        if (request.Category == ProductCategory.Electronics && request.Price < 50m)
        {
            _logger.LogWarning("Electronics product {Name} has price below minimum: {Price}", request.Name, request.Price);
            return false;
        }

        if (request.Category == ProductCategory.Home && !BeAppropriateForHome(request.Name))
        {
            _logger.LogWarning("Home product {Name} contains restricted content.", request.Name);
            return false;
        }

        if (request.Price > 500m && request.StockQuantity > 10)
        {
            _logger.LogWarning("High value product {Name} has stock {Stock} above allowed limit.", request.Name, request.StockQuantity);
            return false;
        }

        if (request.Category == ProductCategory.Electronics)
        {
            if (request.Price < 50m)
            {
                return false;
            }

            if (!ContainTechnologyKeywords(request.Name))
            {
                _logger.LogWarning("Electronics product {Name} does not contain technology keywords.", request.Name);
                return false;
            }

            if (request.ReleaseDate < DateTime.UtcNow.AddYears(-5))
            {
                _logger.LogWarning("Electronics product {Name} is older than 5 years.", request.Name);
                return false;
            }
        }

        if (request.Category == ProductCategory.Home)
        {
            if (request.Price > 200m)
            {
                _logger.LogWarning("Home product {Name} has price above maximum.", request.Name);
                return false;
            }

            if (!BeAppropriateForHome(request.Name))
            {
                return false;
            }
        }

        if (request.Category == ProductCategory.Clothing && (request.Brand?.Length ?? 0) < 3)
        {
            _logger.LogWarning("Clothing product {Name} has brand shorter than 3 characters.", request.Name);
            return false;
        }

        if (request.Price > 100m && request.StockQuantity > 20)
        {
            _logger.LogWarning("Expensive product {Name} has stock {Stock} above 20.", request.Name, request.StockQuantity);
            return false;
        }

        if (request.Category == ProductCategory.Electronics && request.ReleaseDate < DateTime.UtcNow.AddYears(-5))
        {
            return false;
        }

        return true;
    }

    private bool ContainTechnologyKeywords(string name)
    {
        var lower = name.ToLowerInvariant();
        return TechnologyKeywords.Any(k => lower.Contains(k));
    }

    private bool BeAppropriateForHome(string name)
    {
        var lower = name.ToLowerInvariant();
        return !HomeRestrictedWords.Any(w => lower.Contains(w));
    }
}
