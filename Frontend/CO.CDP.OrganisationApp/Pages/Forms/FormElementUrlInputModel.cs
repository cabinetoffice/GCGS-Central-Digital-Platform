using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CO.CDP.Localization;

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
                yield return new ValidationResult(StaticTextResource.Global_RadioField_SelectOptionError, [nameof(HasValue)]);
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
                yield return new ValidationResult(StaticTextResource.Forms_FormElementUrlInput_EnterWebsiteAddressError, [nameof(TextInput)]);
            }
            else
            {
                if (!Uri.TryCreate(TryFixUrl(TextInput), UriKind.Absolute, out _))
                {
                    yield return new ValidationResult(StaticTextResource.Forms_FormElementUrlInput_InvalidWebsiteAddressError, [nameof(TextInput)]);
                }
            }
        }
    }

    private static string TryFixUrl(string url)
    {
        if (!Regex.IsMatch(url, "^(http|https)://") && !url.StartsWith("//"))
        {
            url = $"http://{url}";
        }
        return url.Trim();
    }
}
