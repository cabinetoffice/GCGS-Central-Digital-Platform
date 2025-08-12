using CO.CDP.Localization;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class YearAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success!;
        }

        if (int.TryParse(value.ToString(), out int year))
        {
            int currentYear = DateTimeOffset.UtcNow.Year;
            if (year >= 1900 && year <= currentYear + 100)
            {
                return ValidationResult.Success!;
            }
        }

        return new ValidationResult(ErrorMessage ?? StaticTextResource.Forms_FormElementTextInput_InvalidYearError);
    }
}
