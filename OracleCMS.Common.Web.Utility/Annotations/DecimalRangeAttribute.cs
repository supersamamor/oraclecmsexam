using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace OracleCMS.Common.Web.Utility.Annotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class DecimalRangeAttribute : BaseValidationAttribute
{
    public readonly double Minimum;
    public readonly double Maximum;

    public DecimalRangeAttribute(double minimum, double maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }
    public override string FormatErrorMessage(string name)
    {
        if (ErrorMessage == null && ErrorMessageResourceName == null)
        {
            ErrorMessage = "The field {0} must be between {1} and {2}";
        }
        return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, Minimum, Maximum);
    }
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success!;
        }

        string decimalString = value.ToString()!.Replace(",", "");

        if (!double.TryParse(decimalString, NumberStyles.Any, CultureInfo.InvariantCulture, out double decimalValue))
        {
            return new ValidationResult(ErrorMessage ?? $"The field {validationContext.DisplayName} is not a valid decimal number.");
        }

        if (decimalValue < Minimum || decimalValue > Maximum)
        {
            return new ValidationResult(ErrorMessage ?? $"The field {validationContext.DisplayName} must be between {Minimum} and {Maximum}.");
        }

        return ValidationResult.Success!;
    }
}

public class DecimalRangeAttributeAdapter : AttributeAdapterBase<DecimalRangeAttribute>
{
    public DecimalRangeAttributeAdapter(DecimalRangeAttribute attribute, IStringLocalizer? stringLocalizer)
        : base(attribute, stringLocalizer)
    {
    }
    public override void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-decimalRange", GetErrorMessage(context));
        MergeAttribute(context.Attributes, "data-val-decimalRange-min", FormatDouble(Attribute.Minimum));
        MergeAttribute(context.Attributes, "data-val-decimalRange-max", FormatDouble(Attribute.Maximum));
    }
    public override string GetErrorMessage(ModelValidationContextBase validationContext)
    {
        return Attribute.FormatErrorMessage(validationContext.ModelMetadata.GetDisplayName());
    }
    private static string FormatDouble(double value)
    {
        if (value == 0) return "0";
        // Check for values that are too small for decimal
        if (Math.Abs(value) < (double)decimal.MinValue || Math.Abs(value) > (double)decimal.MaxValue)
        {
            // For values outside the decimal range, use the "G17" format specifier.
            // "G17" provides enough precision to accurately represent a double.
            return value.ToString("G17", CultureInfo.InvariantCulture);
        }
        // Direct approach for values within the decimal range, avoiding scientific notation.
        string stringValue = value.ToString("G", CultureInfo.InvariantCulture);
        // If still in scientific notation, manually format.
        if (stringValue.Contains('E') || stringValue.Contains('e'))
        {
            // Use a custom formatting approach for small numbers to avoid scientific notation.
            // Note: Adjust the number of zeros based on your application's precision requirements.
            return value.ToString("0.##################################################", CultureInfo.InvariantCulture);
        }
        return stringValue;
    }
}