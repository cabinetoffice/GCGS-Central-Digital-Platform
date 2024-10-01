using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementSingleChoiceModel : FormElementModel, IValidatableObject
{
    [BindProperty]
    public string? SelectedOption { get; set; }

    [BindProperty]
    public required string AnswerFieldName { get; set; }

    public override FormAnswer? GetAnswer()
    {
        if(
            SelectedOption != null
            && Options?.Choices != null
            && Options.Choices.ContainsKey(SelectedOption)
         )
        {
            FormAnswer formAnswer;

            switch (AnswerFieldName)
            {
                case nameof(FormAnswer.OptionValue):
                    formAnswer = new FormAnswer { OptionValue = SelectedOption };
                    break;
                case nameof(FormAnswer.JsonValue):
                    formAnswer = new FormAnswer { JsonValue = SelectedOption };
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported answer type: {AnswerFieldName}");
            }

            return formAnswer;

        }

        return null;
    }

    public override void SetAnswer(FormAnswer? answer)
    {
        string? value;

        switch (AnswerFieldName)
        {
            case nameof(FormAnswer.OptionValue):
                value = answer?.OptionValue;
                break;
            case nameof(FormAnswer.JsonValue):
                value = answer?.JsonValue;
                break;
            default:
                throw new InvalidOperationException($"Unsupported answer type: {AnswerFieldName}");
        }

        if (value != null && Options?.Choices != null && Options.Choices.ContainsKey(value))
        {
            SelectedOption = value;
        }
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {

        if (string.IsNullOrWhiteSpace(SelectedOption))
        {
            yield return new ValidationResult("Select an option", new[] { nameof(SelectedOption) });
            yield break;
        }

        if(Options?.Choices == null || (SelectedOption != null && !Options.Choices.ContainsKey(SelectedOption)))
        {
            // Note that any JSON responses received from the client *must* be present in the Choices list, and this check ensures that this is the case.
            yield return new ValidationResult("Invalid option selected", new[] { nameof(SelectedOption) });
            yield break;
        }
    }
}