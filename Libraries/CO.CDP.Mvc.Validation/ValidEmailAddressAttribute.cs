using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Mvc.Validation;

public class ValidEmailAddressAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if(value is string emailAddress && !EmailAddressValidator.ValidateEmailAddress(emailAddress))
        {
            var errorMessage = ErrorMessageResolver.GetErrorMessage(ErrorMessage, ErrorMessageResourceName, ErrorMessageResourceType);

            return new ValidationResult(errorMessage ?? $"{validationContext.DisplayName} is invalid.", [validationContext.MemberName!]);
        }
        return ValidationResult.Success!;
    }
}