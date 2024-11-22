using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Mvc.Validation;

public class ValidUriAttribute : RequiredAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string str && !string.IsNullOrWhiteSpace(str) && Uri.TryCreate(str, UriKind.Absolute, out var _) == false)
        {
            var errorMessage = ErrorMessageResolver.GetErrorMessage(ErrorMessage, ErrorMessageResourceName, ErrorMessageResourceType);
            var displayName = validationContext.DisplayName ?? validationContext.MemberName;

            return new ValidationResult(errorMessage ?? $"{displayName} is invalid");
        }

        return ValidationResult.Success!;
    }
}