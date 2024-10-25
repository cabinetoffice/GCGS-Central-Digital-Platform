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
        FormAnswer? formAnswer = null;

        if (HasValue != null)
        {
            formAnswer = new FormAnswer { BoolValue = HasValue };
        }

        if (HasValue != false && !string.IsNullOrWhiteSpace(TextInput))
        {
            formAnswer ??= new FormAnswer();
            formAnswer.TextValue = TryFixUrl(TextInput);
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
            if (string.IsNullOrWhiteSpace(TextInput))
            {
                yield return new ValidationResult("Enter a website address", [nameof(TextInput)]);
            }
            else
            {
                if (!Uri.TryCreate(TryFixUrl(TextInput), UriKind.Absolute, out _))
                {
                    yield return new ValidationResult("Website address must be in the correct format, like www.companyname.com", [nameof(TextInput)]);
                }
            }
        }
    }

    private static string TryFixUrl(string url)
    {
        if (!url.StartsWith("http") && !url.StartsWith("//"))
        {
            url = $"https://{url}";
        }
        return url.Trim();
    }
}