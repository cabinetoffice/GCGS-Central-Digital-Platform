using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementTextInputModel : FormElementModel, IValidatableObject
{
    [BindProperty] public string? TextInput { get; set; }

    [BindProperty] public bool? HasValue { get; set; }


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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
        ValidateHasValue()
            .Concat(ValidateTextField());

    private IEnumerable<ValidationResult> ValidateHasValue() =>
        !IsRequired && HasValue == null
            ? [new ValidationResult(StaticTextResource.Global_RadioField_SelectOptionError, [nameof(HasValue)])]
            : [];

    private IEnumerable<ValidationResult> ValidateTextField() =>
        ShouldValidateTextField()
            ? ValidateTextInput()
            : [];

    private bool ShouldValidateTextField() =>
        IsRequired || HasValue == true;

    private IEnumerable<ValidationResult> ValidateTextInput() =>
        GetTextValidationRules()
            .Select(rule => rule(TextInput))
            .Where(result => result != null)
            .Cast<ValidationResult>();

    private IEnumerable<Func<string?, ValidationResult?>> GetTextValidationRules() =>
    [
        ValidateEmail,
        ValidateByType,
        ValidateRequired
    ];

    private ValidationResult? ValidateEmail(string? input) =>
        IsEmailValidationRequired() && !IsValidEmail(input)
            ? new ValidationResult(StaticTextResource.Forms_FormElementTextInput_InvalidEmailError, [nameof(TextInput)])
            : null;

    private ValidationResult? ValidateByType(string? input) =>
        Options?.Validation?.TextValidationType switch
        {
            TextValidationType.Year when !new Validation.YearAttribute().IsValid(input) =>
                new ValidationResult(StaticTextResource.Forms_FormElementTextInput_InvalidYearError,
                    [nameof(TextInput)]),
            TextValidationType.Number when !new Validation.NumberAttribute().IsValid(input) =>
                new ValidationResult(StaticTextResource.Global_Number_InvalidError, [nameof(TextInput)]),
            TextValidationType.Percentage when !new Validation.PercentageAttribute().IsValid(input) =>
                new ValidationResult(StaticTextResource.Global_Percentage_InvalidError, [nameof(TextInput)]),
            TextValidationType.Decimal when !new Validation.DecimalAttribute().IsValid(input) =>
                new ValidationResult(StaticTextResource.Global_Decimal_InvalidError, [nameof(TextInput)]),
            _ => null
        };

    private ValidationResult? ValidateRequired(string? input) =>
        string.IsNullOrWhiteSpace(input) && !IsEmailValidationRequired()
            ? new ValidationResult(StaticTextResource.Forms_FormElementTextInput_EnterValueError, [nameof(TextInput)])
            : null;

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