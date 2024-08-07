using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementCheckBoxInputModel : FormElementModel, IValidatableObject
{
    [BindProperty]
    public bool? CheckBoxInput { get; set; }

    public override FormAnswer? GetAnswer()
    {
        return CheckBoxInput.HasValue && CheckBoxInput.Value ? new FormAnswer { BoolValue = CheckBoxInput.Value } : null;
    }

    public override void SetAnswer(FormAnswer? answer)
    {
        if (answer?.BoolValue.HasValue == true)
        {
            CheckBoxInput = answer.BoolValue;
        }
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CurrentFormQuestionType == FormQuestionType.CheckBox && IsRequired == true && CheckBoxInput != true)
        {
            yield return new ValidationResult("You must agree to the declaration statements to proceed.", new[] { nameof(CheckBoxInput) });
        }
    }
}