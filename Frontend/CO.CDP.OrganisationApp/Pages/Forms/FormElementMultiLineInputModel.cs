using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementMultiLineInputModel : FormElementModel, IValidatableObject
{
    [BindProperty] public string? TextInput { get; set; }

    [BindProperty] public bool? HasValue { get; set; }

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
        if (!IsRequired && HasValue == null)
        {
            yield return new ValidationResult(StaticTextResource.Global_RadioField_SelectOptionError, [nameof(HasValue)]);
            yield break;
        }

        if (IsRequired || HasValue == true)
        {
            if (string.IsNullOrWhiteSpace(TextInput))
            {
                yield return new ValidationResult(StaticTextResource.Forms_FormElementMultiLineInput_AllInformationRequiredError, [nameof(TextInput)]);
            }

            if (TextInput != null && TextInput.Length > 10000)
            {
                yield return new ValidationResult(StaticTextResource.Forms_FormElementMultiLineInput_CharacterCountError, [nameof(TextInput)]);
            }
        }
    }
}
