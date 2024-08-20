using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementTextInputModel : FormElementModel, IValidatableObject
{
    [BindProperty]
    public string? TextInput { get; set; }

    public override FormAnswer? GetAnswer()
    {
        return string.IsNullOrWhiteSpace(TextInput) ? null : new FormAnswer { TextValue = TextInput };
    }

    public override void SetAnswer(FormAnswer? answer)
    {
        if (answer?.TextValue != null)
        {
            TextInput = answer.TextValue;
        }
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {

        if (IsRequired == true && string.IsNullOrWhiteSpace(TextInput))
        {
            yield return new ValidationResult("All information is required on this page", new[] { nameof(TextInput) });
        }
        else if (!string.IsNullOrWhiteSpace(TextInput) && IsEmailValidationRequired() && !IsValidEmail(TextInput))
        {
            yield return new ValidationResult("Enter an email address in the correct format, like name@example.com.", new[] { nameof(TextInput) });
        }
    }
    private bool IsEmailValidationRequired()
    {
        return Heading?.Contains("email", StringComparison.OrdinalIgnoreCase) == true;
    }

    private bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailRegex);
    }
}