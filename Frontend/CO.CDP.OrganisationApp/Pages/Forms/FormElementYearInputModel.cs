using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Constants;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementYearInputModel : FormElementModel, IValidatableObject
{
    [BindProperty] public string? Year { get; set; }

    [BindProperty] public bool? HasValue { get; set; }

    public override FormAnswer? GetAnswer()
    {
        FormAnswer? formAnswer = null;

        if (HasValue != null)
        {
            formAnswer = new FormAnswer { BoolValue = HasValue };
        }

        if (HasValue != false && !string.IsNullOrWhiteSpace(Year))
        {
            formAnswer ??= new FormAnswer();
            formAnswer.TextValue = Year;
        }

        return formAnswer;
    }

    public override void SetAnswer(FormAnswer? answer)
    {
        if (answer == null) return;

        HasValue = answer.BoolValue;
        Year = answer.TextValue;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CurrentFormQuestionType != FormQuestionType.Year)
        {
            yield break;
        }

        var validateField = IsRequired;

        if (IsRequired == false)
        {
            if (HasValue == null)
            {
                yield return new ValidationResult(StaticTextResource.Global_RadioField_SelectOptionError,
                    new[] { nameof(HasValue) });
            }
            else if (HasValue == true)
            {
                validateField = true;
            }
        }

        if (validateField)
        {
            if (string.IsNullOrWhiteSpace(Year))
            {
                yield return new ValidationResult(FormsEngineResource.Global_YearInput_YearRequiredError,
                    new[] { nameof(Year) });
            }
            else if (!Regex.IsMatch(Year, RegExPatterns.Year))
            {
                yield return new ValidationResult(FormsEngineResource.Global_YearInput_YearInvalidError,
                    new[] { nameof(Year) });
            }
        }
    }
}