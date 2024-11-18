using CO.CDP.Localization;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace CO.CDP.OrganisationApp.Validation;

public class ValidEmailAddressAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string email && !MailAddress.TryCreate(email, out var _))
        {
            var stringLocalizer = validationContext.GetService<IServiceProvider>()
            ?.GetService<IStringLocalizerFactory>()
            ?.Create(typeof(StaticTextResource));

            var errorMessage = !string.IsNullOrWhiteSpace(ErrorMessageResourceName) && stringLocalizer != null
                ? stringLocalizer[ErrorMessageResourceName]
                : $"{validationContext.MemberName} is invalid.";

            return new ValidationResult(errorMessage, [validationContext.MemberName]);
        }
        return ValidationResult.Success!;
    }
}