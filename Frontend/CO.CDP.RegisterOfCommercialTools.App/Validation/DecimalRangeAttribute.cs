using System.ComponentModel.DataAnnotations;

namespace CO.CDP.RegisterOfCommercialTools.App.Validation;

public class DecimalRangeAttribute(string compareToPropertyName) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var property = validationContext.ObjectType.GetProperty(compareToPropertyName);

        if (property == null)
        {
            return new ValidationResult($"Unknown property: {compareToPropertyName}");
        }

        var compareToValue = property.GetValue(validationContext.ObjectInstance);

        if (value is not decimal || compareToValue is not decimal)
        {
            return ValidationResult.Success;
        }

        if ((decimal)value < (decimal)compareToValue)
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}