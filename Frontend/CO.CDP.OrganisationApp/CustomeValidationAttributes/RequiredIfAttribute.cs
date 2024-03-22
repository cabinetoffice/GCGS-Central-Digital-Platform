using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.CustomeValidationAttributes;

public class RequiredIfAttribute(string dependentProperty, object targetValue) : RequiredAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var field = validationContext.ObjectType.GetProperty(dependentProperty);
        if (field != null)
        {
            var dependentValue = field.GetValue(validationContext.ObjectInstance, null);

            if (dependentValue == null && targetValue == null || dependentValue != null && dependentValue.Equals(targetValue))
            {
                var requiredAttribute = new RequiredAttribute();

                if (!requiredAttribute.IsValid(value))
                {
                    string name = validationContext.DisplayName! ?? validationContext.MemberName!;

                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        ErrorMessage = name + " is required";
                    }

                    return new ValidationResult(ErrorMessage, new List<string>() { name });
                }
            }

            return ValidationResult.Success;
        }

        return new ValidationResult(FormatErrorMessage(dependentProperty));
    }
}