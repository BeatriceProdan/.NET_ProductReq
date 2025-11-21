using System.ComponentModel.DataAnnotations;
using ProductsManagement.Features.Products;

namespace ProductsManagement.Validators.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ProductCategoryAttribute : ValidationAttribute
{
    private readonly ProductCategory[] _allowedCategories;

    public ProductCategoryAttribute(params ProductCategory[] allowedCategories)
    {
        _allowedCategories = allowedCategories;
    }

    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return false;
        }

        if (value is not ProductCategory category)
        {
            return false;
        }

        return _allowedCategories.Contains(category);
    }

    public override string FormatErrorMessage(string name)
    {
        var allowed = string.Join(", ", _allowedCategories.Select(c => c.ToString()));
        return ErrorMessage ?? $"{name} must be one of: {allowed}.";
    }
}
