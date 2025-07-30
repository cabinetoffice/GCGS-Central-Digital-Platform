using CO.CDP.Localization;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class DateRangeAttribute : ValidationAttribute
{
    private readonly DateTimeOffset _minDate;
    private readonly DateTimeOffset _maxDate;

    public DateRangeAttribute(string minDate, string maxDate)
    {
        _minDate = DateTimeOffset.Parse(minDate);
        _maxDate = DateTimeOffset.Parse(maxDate);
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTimeOffset date)
        {
            if (date >= _minDate && date <= _maxDate)
            {
                return ValidationResult.Success!;
            }
            return new ValidationResult(ErrorMessage ?? string.Format(StaticTextResource.Global_DateInput_DateMustBeBetween, _minDate.Date.ToShortDateString(), _maxDate.Date.ToShortDateString()));
        }
        return ValidationResult.Success!;
    }
}
