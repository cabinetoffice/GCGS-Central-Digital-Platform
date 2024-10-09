using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementTextInputModel : FormElementModel, IValidatableObject
{
    [BindProperty]
    public string? TextInput { get; set; }

    [BindProperty]
    public bool? HasValue { get; set; }


    public override FormAnswer? GetAnswer()
    {
        FormAnswer? formAnswer = null;

        if (HasValue != null)
        {
            formAnswer = new FormAnswer { BoolValue = HasValue };
        }

        if (!string.IsNullOrWhiteSpace(TextInput))
        {
            formAnswer ??= new FormAnswer();
            formAnswer.TextValue = TextInput;
        }

        return formAnswer;
    }

    public override void SetAnswer(FormAnswer? answer)
    {
        if (answer == null) return;

        HasValue = answer.BoolValue;
        TextInput = answer.TextValue;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validateTextField = IsRequired;

        if (IsRequired == false)
        {
            if (HasValue == null)
            {
                yield return new ValidationResult("Select an option", [nameof(HasValue)]);
            }
            else if (HasValue == true)
            {
                validateTextField = true;
            }
        }

        if (validateTextField)
        {
            if (IsEmailValidationRequired() && !IsValidEmail(TextInput))
            {
                yield return new ValidationResult("Enter an email address in the correct format, like name@example.com.", [nameof(TextInput)]);
            }
            else if (string.IsNullOrWhiteSpace(TextInput))
            {
                yield return new ValidationResult("Enter a value", [nameof(TextInput)]);
            }
        }
    }
    private bool IsEmailValidationRequired()
    {
        return Heading?.Contains("email", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailRegex);
    }
}