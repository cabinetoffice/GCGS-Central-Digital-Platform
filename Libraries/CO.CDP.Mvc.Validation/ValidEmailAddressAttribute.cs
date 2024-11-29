using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Mvc.Validation;

public class ValidEmailAddressAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string emailAddress && !EmailAddressValidator.IsValid(emailAddress))
        {
            var errorMessage = ErrorMessageResolver.GetErrorMessage(ErrorMessage, ErrorMessageResourceName, ErrorMessageResourceType)
                ?? $"{validationContext.DisplayName} is invalid.";

            return new ValidationResult(errorMessage, [validationContext.MemberName!]);
        }

        return ValidationResult.Success!;
    }
}