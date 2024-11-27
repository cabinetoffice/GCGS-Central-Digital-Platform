using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementYesNoInputModel : FormElementModel, IValidatableObject
{
    [BindProperty]
    public string? YesNoInput { get; set; }

    public override FormAnswer? GetAnswer()
    {
        return string.IsNullOrWhiteSpace(YesNoInput) ? null : new FormAnswer { BoolValue = YesNoInput == "yes" };
    }

    public override void SetAnswer(FormAnswer? answer)
    {
        if (answer?.BoolValue != null)
        {
            YesNoInput = answer.BoolValue.Value ? "yes" : "no";
        }
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CurrentFormQuestionType == FormQuestionType.YesOrNo && IsRequired == true && string.IsNullOrWhiteSpace(YesNoInput))
        {
            yield return new ValidationResult(StaticTextResource.Global_RadioField_SelectOptionError, [nameof(YesNoInput)]);
        }
    }
}
