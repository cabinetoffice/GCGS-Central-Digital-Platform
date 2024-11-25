using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace CO.CDP.Mvc.Validation;

public class ValidEmailAddressAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string email && !MailAddress.TryCreate(email, out var _))
        {
            var errorMessage = ErrorMessageResolver.GetErrorMessage(ErrorMessage, ErrorMessageResourceName, ErrorMessageResourceType);

            return new ValidationResult(errorMessage ?? $"{validationContext.DisplayName} is invalid.", [validationContext.MemberName!]);
        }
        return ValidationResult.Success!;
    }
}