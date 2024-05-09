using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Common;

public class RequiredIfAttribute(string dependentProperty, object? targetValue) : RequiredAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        var instance = validationContext.ObjectInstance;
        var type = instance.GetType();
        var propertyValue = type.GetProperty(dependentProperty)?.GetValue(instance, null);

        bool conditionMet = (propertyValue?.Equals(targetValue) ?? false);

        if (conditionMet && (value == null || (value is string str && string.IsNullOrWhiteSpace(str))))
        {
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required");
        }

        return ValidationResult.Success!;
    }
}