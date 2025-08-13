using System.ComponentModel.DataAnnotations;

namespace CO.CDP.RegisterOfCommercialTools.App.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class DateRangeAttribute(string compareToPropertyName) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not DateOnly toDate)
        {
            return ValidationResult.Success;
        }

        var fromDateProperty = validationContext.ObjectType.GetProperty(compareToPropertyName);
        if (fromDateProperty == null)
        {
            return ValidationResult.Success;
        }

        var fromDateValue = fromDateProperty.GetValue(validationContext.ObjectInstance);
        if (fromDateValue is not DateOnly fromDate)
        {
            return ValidationResult.Success;
        }

        if (toDate < fromDate)
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}