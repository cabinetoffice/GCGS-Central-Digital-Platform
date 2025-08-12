using CO.CDP.Localization;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class MaxDateAttribute : ValidationAttribute
{
    private readonly DateTimeOffset _maxDate;

    public MaxDateAttribute(string maxDate)
    {
        _maxDate = DateTimeOffset.Parse(maxDate);
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTimeOffset date)
        {
            if (date <= _maxDate)
            {
                return ValidationResult.Success!;
            }
            return new ValidationResult(ErrorMessage ?? string.Format(StaticTextResource.Global_DateInput_DateMustBeOnOrBefore, _maxDate.Date.ToShortDateString()));
        }
        return ValidationResult.Success!;
    }
}
