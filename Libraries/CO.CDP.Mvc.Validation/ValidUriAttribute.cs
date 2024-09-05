using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Mvc.Validation;

public class ValidUriAttribute : RequiredAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string str && !string.IsNullOrWhiteSpace(str) && Uri.TryCreate(str, UriKind.Absolute, out var _) == false)
        {
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is invalid");
        }

        return ValidationResult.Success!;
    }
}