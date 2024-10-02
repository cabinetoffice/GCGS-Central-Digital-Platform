using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementUrlInputModel : FormElementModel, IValidatableObject
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
            if (string.IsNullOrWhiteSpace(TextInput))
            {
                yield return new ValidationResult("Enter a website address", [nameof(TextInput)]);
            }
            else if (Uri.TryCreate(TextInput, UriKind.Absolute, out var _) == false)
            {
                yield return new ValidationResult("Enter a valid website address in the correct format", [nameof(TextInput)]);
            }
        }
    }
}