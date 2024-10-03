using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementGroupedSingleChoiceModel : FormElementModel, IValidatableObject
{
    [BindProperty]
    public string? SelectedOption { get; set; }

    public override FormAnswer? GetAnswer()
    {
        if (SelectedOption != null && Options?.Groups != null)
        {
            var isValidOption = Options.Groups.Any(group => group.Choices != null && group.Choices.Any(choice => choice.Value == SelectedOption));

            if (isValidOption)
            {
                return new FormAnswer { OptionValue = SelectedOption };
            }
        }

        return null;
    }

    public override void SetAnswer(FormAnswer? answer)
    {
        if (answer?.OptionValue != null && Options?.Groups != null)
        {
            var isValidOption = Options.Groups.Any(group => group.Choices != null && group.Choices.Any(choice => choice.Value == answer.OptionValue));

            if (isValidOption)
            {
                SelectedOption = answer.OptionValue;
            }
        }
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(SelectedOption))
        {
            yield return new ValidationResult("Select an option", new[] { nameof(SelectedOption) });
            yield break;
        }

        if (Options?.Groups == null || !Options.Groups.Any(group => group.Choices != null && group.Choices.Any(choice => choice.Value == SelectedOption)))
        {
            yield return new ValidationResult("Invalid option selected", new[] { nameof(SelectedOption) });
        }
    }
}