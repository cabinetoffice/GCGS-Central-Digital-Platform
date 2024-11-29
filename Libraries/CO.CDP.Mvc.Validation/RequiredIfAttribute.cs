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
    private readonly string _organisationSchemeProperty;

    public RequiredIfHasValueAttribute(string organisationSchemeProperty)
    {
        _organisationSchemeProperty = organisationSchemeProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var model = validationContext.ObjectInstance;
        var dependentProperty = model.GetType().GetProperty(_organisationSchemeProperty);
        var dependentPropertyValue = dependentProperty?.GetValue(model) as string;

        var errorMessage = ErrorMessageResolver.GetErrorMessage(ErrorMessage, ErrorMessageResourceName, ErrorMessageResourceType);

        if (value is Dictionary<string, string?> collection)
        {
            if (!string.IsNullOrEmpty(dependentPropertyValue) &&
                collection.TryGetValue(dependentPropertyValue, out var registrationNumberValue))
            {
                if (string.IsNullOrEmpty(registrationNumberValue))
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

        // Check if value is null or empty when condition is met
        if (conditionMet && (value == null || (value is List<string> str && str.Count == 0)))
        {
            return new ValidationResult(errorMessage ?? $"{validationContext.DisplayName} is required");
        }

        return ValidationResult.Success;
    }
}