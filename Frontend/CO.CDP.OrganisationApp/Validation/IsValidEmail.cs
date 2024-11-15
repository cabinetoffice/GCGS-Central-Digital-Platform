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

            var errorMessage = !string.IsNullOrWhiteSpace(ErrorMessage) && stringLocalizer != null
                ? stringLocalizer[ErrorMessage]
                : $"{validationContext.MemberName} is invalid.";

            return new ValidationResult(errorMessage);
        }
        return ValidationResult.Success!;
    }
}