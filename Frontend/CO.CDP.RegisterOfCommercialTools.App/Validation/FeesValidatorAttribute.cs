using System.ComponentModel.DataAnnotations;

namespace CO.CDP.RegisterOfCommercialTools.App.Validation;

[AttributeUsage(AttributeTargets.Class)]
public class FeesValidatorAttribute(string feeFrom, string feeTo, string noFees) : ValidationAttribute
{
    public string FeeFrom { get; } = feeFrom;
    public string FeeTo { get; } = feeTo;
    public string NoFees { get; } = noFees;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var feeFromProperty = validationContext.ObjectType.GetProperty(FeeFrom);
        var feeToProperty = validationContext.ObjectType.GetProperty(FeeTo);
        var noFeesProperty = validationContext.ObjectType.GetProperty(NoFees);

        if (feeFromProperty == null || feeToProperty == null || noFeesProperty == null)
        {
            return new ValidationResult("Invalid property names provided to the FeesValidatorAttribute.");
        }

        var feeFromValue = feeFromProperty.GetValue(validationContext.ObjectInstance) as decimal?;
        var feeToValue = feeToProperty.GetValue(validationContext.ObjectInstance) as decimal?;
        var noFeesValue = noFeesProperty.GetValue(validationContext.ObjectInstance) as bool?;

        if (noFeesValue == true && (feeFromValue.HasValue || feeToValue.HasValue))
        {
            return new ValidationResult("Fee values cannot be provided when 'No fees' is selected.",
                [FeeFrom, FeeTo]);
        }

        return ValidationResult.Success;
    }
}

