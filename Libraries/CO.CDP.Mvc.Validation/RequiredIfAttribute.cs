using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Mvc.Validation;

public class RequiredIfAttribute(string dependentProperty, object? targetValue) : RequiredAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        var instance = validationContext.ObjectInstance;
        var type = instance.GetType();
        var propertyValue = type.GetProperty(dependentProperty)?.GetValue(instance, null);

        bool conditionMet = (propertyValue?.Equals(targetValue) ?? false);

        var errorMessage = ErrorMessageResolver.GetErrorMessage(ErrorMessage, ErrorMessageResourceName, ErrorMessageResourceType);

        if (conditionMet && (value == null || (value is string str && string.IsNullOrWhiteSpace(str))))
        {
            return new ValidationResult(errorMessage ?? $"{validationContext.DisplayName} is required");
        }

        return ValidationResult.Success!;
    }
}

public class RequiredIfHasValueAttribute : ValidationAttribute
{
    private readonly string _dependentProperty;

    public RequiredIfHasValueAttribute(string dependentProperty)
    {
        _dependentProperty = dependentProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var model = validationContext.ObjectInstance;
        var dependentProperty = model.GetType().GetProperty(_dependentProperty);
        var dependentPropertyValue = dependentProperty?.GetValue(model) as string;

        var errorMessage = ErrorMessageResolver.GetErrorMessage(ErrorMessage, ErrorMessageResourceName, ErrorMessageResourceType);

        if (value is Dictionary<string, string?> collection)
        {
            if (!string.IsNullOrEmpty(dependentPropertyValue) &&
                collection.TryGetValue(dependentPropertyValue, out var dependentPropertyValueData))
            {
                if (string.IsNullOrEmpty(dependentPropertyValueData))
                {                    
                    return new ValidationResult(errorMessage ?? $"{validationContext.DisplayName} is required");
                }
            }
        }
        else if (!string.IsNullOrEmpty(dependentPropertyValue) && value == null)
        {
            return new ValidationResult(errorMessage ?? $"{validationContext.DisplayName} is required");
        }

        return ValidationResult.Success;
    }   
}

public class RequiredIfContainsAttribute(string dependentProperty, string containsValue) : RequiredAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var instance = validationContext.ObjectInstance;
        var type = instance.GetType();
        var propertyValue = type.GetProperty(dependentProperty)?.GetValue(instance, null);

        bool conditionMet = propertyValue is List<string> list && list.Contains(containsValue);

        var errorMessage = ErrorMessageResolver.GetErrorMessage(ErrorMessage, ErrorMessageResourceName, ErrorMessageResourceType);
        
        if (conditionMet && (value == null || (value is List<string> str && str.Count == 0)))
        {
            return new ValidationResult(errorMessage ?? $"{validationContext.DisplayName} is required");
        }

        return ValidationResult.Success;
    }
}