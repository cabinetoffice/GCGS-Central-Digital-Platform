using CO.CDP.Localization;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class MinDateAttribute : ValidationAttribute
{
    private readonly DateTimeOffset _minDate;

    public MinDateAttribute(string minDate)
    {
        _minDate = DateTimeOffset.Parse(minDate);
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTimeOffset date)
        {
            if (date >= _minDate)
            {
                return ValidationResult.Success!;
            }
            return new ValidationResult(ErrorMessage ?? string.Format(StaticTextResource.Global_DateInput_DateMustBeOnOrAfter, _minDate.Date.ToShortDateString()));
        }
        return ValidationResult.Success!;
    }
}
