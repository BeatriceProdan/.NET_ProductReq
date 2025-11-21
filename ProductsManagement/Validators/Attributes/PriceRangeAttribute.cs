using System.ComponentModel.DataAnnotations;

namespace ProductsManagement.Validators.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class PriceRangeAttribute : ValidationAttribute
{
    private readonly decimal _min;
    private readonly decimal _max;

    public PriceRangeAttribute(double min, double max)
    {
        _min = (decimal)min;
        _max = (decimal)max;
    }

    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            return true;
        }

        if (value is not decimal d)
        {
            return false;
        }

        return d >= _min && d <= _max;
    }

    public override string FormatErrorMessage(string name)
    {
        return ErrorMessage ?? $"{name} must be between {_min:C2} and {_max:C2}.";
    }
}
