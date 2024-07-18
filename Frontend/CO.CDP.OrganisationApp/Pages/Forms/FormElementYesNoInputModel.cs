using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementYesNoInputModel : FormElementModel, IValidatableObject
{
    [BindProperty]
    public string? YesNoInput { get; set; }

    public override string? GetAnswer()
    {
        return YesNoInput;
    }

    public override void SetAnswer(string? answer)
    {
        YesNoInput = answer;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FormQuestionType == Models.FormQuestionType.YesOrNo && IsRequired == true && string.IsNullOrWhiteSpace(YesNoInput))
        {
            yield return new ValidationResult("Please select an option.", [nameof(YesNoInput)]);
        }
    }
}