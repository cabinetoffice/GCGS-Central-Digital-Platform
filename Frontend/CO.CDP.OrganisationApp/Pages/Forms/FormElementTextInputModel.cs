using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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
        if (FormQuestionType == Models.FormQuestionType.Text && IsRequired == true && string.IsNullOrWhiteSpace(TextInput))
        {
            yield return new ValidationResult("Please provide a value.", [nameof(TextInput)]);
        }
    }
}