using CO.CDP.Localization;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTimeOffset date)
        {
            if (date.Date > DateTimeOffset.UtcNow.Date)
            {
                return ValidationResult.Success!;
            }
            return new ValidationResult(ErrorMessage ?? StaticTextResource.Global_DateInput_DateInFutureValidation);
        }
        return ValidationResult.Success!;
    }
}
