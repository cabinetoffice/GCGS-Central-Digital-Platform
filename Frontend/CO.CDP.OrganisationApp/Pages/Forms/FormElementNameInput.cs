using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementNameInputModel : FormElementModel, IValidatableObject
{
    [BindProperty]
    public string? NameInput { get; set; }


    public override FormAnswer? GetAnswer()
    {
        return string.IsNullOrWhiteSpace(NameInput) ? null : new FormAnswer { TextValue = NameInput };
    }

    public override void SetAnswer(FormAnswer? answer)
    {
        if (answer?.TextValue != null)
        {
            NameInput = answer.TextValue;
        }
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CurrentFormQuestionType == FormQuestionType.NameInput && IsRequired == true && string.IsNullOrWhiteSpace(NameInput))
        {
            yield return new ValidationResult("Please provide a name", [nameof(NameInput)]);
        }
    }
}