using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace OracleCMS.Common.Web.Utility.Annotations;

/// <summary>
/// Specifies that the value of the field should be less than or equal to the value of another field.
/// </summary>
public class LessThanEqualAttribute : BaseValidationAttribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="LessThanEqualAttribute"/>.
    /// </summary>
    /// <param name="otherProperty">The name of the other field that this field should be compared to</param>
    public LessThanEqualAttribute(string otherProperty)
    {
        OtherProperty = otherProperty;
    }

    /// <summary>
    /// The name of the other field that this field should be compared to.
    /// </summary>
    public string OtherProperty { get; private set; }

    /// <summary>
    /// The value of the <see cref="DisplayAttribute"/> of the other field.
    /// </summary>
    public string? OtherPropertyDisplayName { get; set; }

    /// <summary>
    /// Provides the formatted error message string.
    /// </summary>
    /// <param name="name">The name of the field</param>
    /// <returns>The formatted error message string</returns>
    public override string FormatErrorMessage(string name)
    {
        if (ErrorMessage == null && ErrorMessageResourceName == null)
        {
            ErrorMessage = "{0} must be less than or equal to {1}";
        }
        return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name,
                             OtherPropertyDisplayName ?? OtherProperty);
    }

    /// <summary>
    /// Validates if the value of this field is less than or equal to the value of the other field.
    /// </summary>
    /// <param name="value">The value of this field</param>
    /// <param name="validationContext"></param>
    /// <returns>The result of the validation</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }
        var otherPropertyInfo = validationContext.ObjectType.GetRuntimeProperty(OtherProperty);
        if (otherPropertyInfo == null)
        {
            return new ValidationResult($"Unknown property {OtherProperty}");
        }
        OtherPropertyDisplayName = GetDisplayNameForProperty(otherPropertyInfo);
        var otherPropertyValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);
        if (((IComparable)value).CompareTo(otherPropertyValue) > 0)
        {
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
        }
        return ValidationResult.Success;
    }
}

/// <summary>
/// The attribute adapater for <see cref="LessThanEqualAttribute"/> to enable client-side validation.
/// </summary>
public class LessThanEqualAttributeAdapter : AttributeAdapterBase<LessThanEqualAttribute>
{
    /// <summary>
    /// Initializes a new instance of <see cref="LessThanEqualAttributeAdapter"/>.
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="stringLocalizer"></param>
    public LessThanEqualAttributeAdapter(LessThanEqualAttribute attribute, IStringLocalizer? stringLocalizer)
        : base(attribute, stringLocalizer)
    {
    }

    /// <summary>
    /// Adds the necessary attributes to the client-side code.
    /// </summary>
    /// <param name="context"></param>
    public override void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-lessThanEqual", GetErrorMessage(context));
        var property = Attribute.OtherProperty;
        MergeAttribute(context.Attributes, "data-val-lessThanEqual-other", property);
    }

    /// <summary>
    /// Provides the error message for the client-side validation.
    /// </summary>
    /// <param name="validationContext"></param>
    /// <returns>The error message</returns>
    public override string GetErrorMessage(ModelValidationContextBase validationContext)
    {
        Attribute.OtherPropertyDisplayName = validationContext.MetadataProvider.GetMetadataForProperties(validationContext.ModelMetadata.ContainerType!)
                                                                               .FirstOrDefault(x => x.Name == Attribute.OtherProperty)?.DisplayName;
        return Attribute.FormatErrorMessage(validationContext.ModelMetadata.GetDisplayName());
    }
}