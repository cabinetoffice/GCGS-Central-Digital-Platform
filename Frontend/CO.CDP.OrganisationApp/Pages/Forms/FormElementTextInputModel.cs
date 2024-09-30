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
        if (IsRequired == false && HasValue == false)
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(TextInput) ? null : new FormAnswer { TextValue = TextInput };
    }

    public override void SetAnswer(FormAnswer? answer)
    {
        if (answer?.TextValue != null)
        {
            TextInput = answer.TextValue;
            HasValue = true;
        }
        else if (RedirectFromCheckYourAnswerPage && IsRequired == false)
        {
            HasValue = false;
        }
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CurrentFormQuestionType != FormQuestionType.Text)
        {
            yield break;
        }

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