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
        if(
            SelectedOption != null
            && Options?.Choices != null
            && Options.Choices.Contains(SelectedOption)
         )
        {
            return new FormAnswer { OptionValue = SelectedOption };
        }

        return null;
    }

    public override void SetAnswer(FormAnswer? answer)
    {
        if (answer?.OptionValue != null && Options?.Choices != null && Options.Choices.Contains(answer.OptionValue))
        {
            SelectedOption = answer.OptionValue;
        }
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {

        if (string.IsNullOrWhiteSpace(SelectedOption))
        {
            yield return new ValidationResult("Select an option", new[] { nameof(SelectedOption) });
            yield break;
        }

        if(Options?.Choices == null || (SelectedOption != null && !Options.Choices.Contains(SelectedOption)))
        {
            yield return new ValidationResult("Invalid option selected", new[] { nameof(SelectedOption) });
            yield break;
        }
    }
}