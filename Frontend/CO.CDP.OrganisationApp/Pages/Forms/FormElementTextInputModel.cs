using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementTextInputModel : FormElementModel, IValidatableObject
{
    [BindProperty]
    public string? TextInput { get; set; }

    [BindProperty]
    public bool? HasValue { get; set; }


    public override FormAnswer? GetAnswer()
    {
        FormAnswer? formAnswer = null;

        if (HasValue != null)
        {
            formAnswer = new FormAnswer { BoolValue = HasValue };
        }

        if (HasValue != false && !string.IsNullOrWhiteSpace(TextInput))
        {
            formAnswer ??= new FormAnswer();
            formAnswer.TextValue = TextInput;
        }

        return formAnswer;
    }

    public override void SetAnswer(FormAnswer? answer)
    {
        if (answer == null) return;

        HasValue = answer.BoolValue;
        TextInput = answer.TextValue;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validateTextField = IsRequired;

        if (IsRequired == false)
        {
            if (HasValue == null)
            {
                yield return new ValidationResult(StaticTextResource.Global_RadioField_SelectOptionError, [nameof(HasValue)]);
            }
            else if (HasValue == true)
            {
                validateTextField = true;
            }
        }

        if (validateTextField)
        {
            if (IsEmailValidationRequired() && !IsValidEmail(TextInput))
            {
                yield return new ValidationResult(StaticTextResource.Forms_FormElementTextInput_InvalidEmailError, [nameof(TextInput)]);
            }
            else if (Options?.Validation?.TextValidationType == TextValidationType.Year)
            {
                var yearAttribute = new Validation.YearAttribute();
                if (!yearAttribute.IsValid(TextInput))
                {
                    yield return new ValidationResult(StaticTextResource.Forms_FormElementTextInput_InvalidYearError, [nameof(TextInput)]);
                }
            }
            else if (Options?.Validation?.TextValidationType == TextValidationType.Number)
            {
                var numberAttribute = new Validation.NumberAttribute();
                if (!numberAttribute.IsValid(TextInput))
                {
                    yield return new ValidationResult(StaticTextResource.Global_Number_InvalidError, [nameof(TextInput)]);
                }
            }
            else if (Options?.Validation?.TextValidationType == TextValidationType.Percentage)
            {
                var percentageAttribute = new Validation.PercentageAttribute();
                if (!percentageAttribute.IsValid(TextInput))
                {
                    yield return new ValidationResult(StaticTextResource.Global_Percentage_InvalidError, [nameof(TextInput)]);
                }
            }
            else if (string.IsNullOrWhiteSpace(TextInput))
            {
                yield return new ValidationResult(StaticTextResource.Forms_FormElementTextInput_EnterValueError, [nameof(TextInput)]);
            }
        }
    }
    private bool IsEmailValidationRequired()
    {
        return Heading?.Contains("email", StringComparison.OrdinalIgnoreCase) == true;
    }

    public string GetFieldType()
    {
        return IsEmailValidationRequired() ? "email" : "text";
    }

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailRegex);
    }
}
