using CO.CDP.Localization;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CO.CDP.OrganisationApp.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class NumberAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success!;
        }

        if (decimal.TryParse(value.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out _))
        {
            return ValidationResult.Success!;
        }

        return new ValidationResult(ErrorMessage ?? StaticTextResource.Global_Number_InvalidError);
    }
}