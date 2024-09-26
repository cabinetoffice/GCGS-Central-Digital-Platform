using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementSingleChoiceModel : FormElementModel, IValidatableObject
{
    [BindProperty]
    public string? SelectedOption { get; set; }

    public override FormAnswer? GetAnswer()
    {
        return string.IsNullOrWhiteSpace(SelectedOption) ? null : new FormAnswer { OptionValue = SelectedOption };
    }

    public override void SetAnswer(FormAnswer? answer)
    {
        if (answer?.OptionValue != null)
        {
            SelectedOption = answer.OptionValue;
        }
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {

        if (string.IsNullOrWhiteSpace(SelectedOption))
        {
            yield return new ValidationResult("All information is required on this page", new[] { nameof(SelectedOption) });
        }
    }
}