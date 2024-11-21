using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace CO.CDP.Mvc.Validation;

public class ValidEmailAddressAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string email && !MailAddress.TryCreate(email, out var _))
        {
            string? errorMessage = null;

            if(ErrorMessageResourceName != null && ErrorMessageResourceType != null)
            {
                var stringLocalizer = validationContext.GetService<IServiceProvider>()
                    ?.GetService<IStringLocalizerFactory>()
                    ?.Create(ErrorMessageResourceType);

                errorMessage = stringLocalizer?[ErrorMessageResourceName]!;
            }
            errorMessage ??= ErrorMessage ?? $"{validationContext.MemberName} is invalid.";

            return new ValidationResult(errorMessage, validationContext.MemberName != null ? [validationContext.MemberName] : null);
        }
        return ValidationResult.Success!;
    }
}