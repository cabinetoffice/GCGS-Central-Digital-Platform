using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementTextInputModel : FormElementModel, IValidatableObject
{
    [BindProperty]
    public string? TextInput { get; set; }

    public override string? GetAnswer()
    {
        return TextInput;
    }

    public override void SetAnswer(string? answer)
    {
        TextInput = answer;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FormQuestionType == Models.FormQuestionType.Text && IsRequired == true && string.IsNullOrWhiteSpace(TextInput))
        {
            yield return new ValidationResult("Please provide a value.", [nameof(TextInput)]);
        }
    }
}