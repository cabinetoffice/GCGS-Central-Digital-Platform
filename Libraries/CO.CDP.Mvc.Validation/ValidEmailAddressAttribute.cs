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
            var errorMessage = ErrorMessage ?? $"{validationContext.MemberName} is invalid.";

            if(ErrorMessageResourceName != null && ErrorMessageResourceType != null)
            {
                var stringLocalizer = validationContext.GetRequiredService<IServiceProvider>()
                    .GetRequiredService<IStringLocalizerFactory>()
                    .Create(ErrorMessageResourceType);

                errorMessage = stringLocalizer[ErrorMessageResourceName];
            }

            return new ValidationResult(errorMessage, [validationContext.MemberName!]);
        }
        return ValidationResult.Success!;
    }
}