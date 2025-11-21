using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProductsManagement.Validators.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ValidSKUAttribute : ValidationAttribute, IClientModelValidator
{
    private static readonly Regex SkuRegex = new("^[A-Za-z0-9-]{5,20}$", RegexOptions.Compiled);

    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            return true;
        }

        var sku = value.ToString()!.Replace(" ", string.Empty);
        if (string.IsNullOrWhiteSpace(sku))
        {
            return false;
        }

        return SkuRegex.IsMatch(sku);
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-validsku", ErrorMessage ?? "SKU must be alphanumeric, 5-20 characters, and may contain hyphens.");
        MergeAttribute(context.Attributes, "data-val-validsku-pattern", SkuRegex.ToString());
    }

    private static void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (!attributes.ContainsKey(key))
        {
            attributes.Add(key, value);
        }
    }
}
