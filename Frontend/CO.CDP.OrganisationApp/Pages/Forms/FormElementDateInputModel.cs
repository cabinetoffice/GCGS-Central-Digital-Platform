using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementDateInputModel : FormElementModel, IValidatableObject
{
    [BindProperty]
    public string? DateString { get; set; }

    [BindProperty]
    public string? Day { get; set; }

    [BindProperty]
    public string? Month { get; set; }

    [BindProperty]
    public string? Year { get; set; }

    [BindProperty]
    public bool? HasValue { get; set; }

    public override FormAnswer? GetAnswer()
    {
        FormAnswer? formAnswer = null;

        if (HasValue != null)
        {
            formAnswer = new FormAnswer { BoolValue = HasValue };
        }

        if (HasValue != false && !string.IsNullOrWhiteSpace(Day) && !string.IsNullOrWhiteSpace(Month) && !string.IsNullOrWhiteSpace(Year))
        {
            var dateString = $"{Year}-{Month!.PadLeft(2, '0')}-{Day!.PadLeft(2, '0')}";
            if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                formAnswer ??= new FormAnswer();
                formAnswer.DateValue = parsedDate;
            }
        }

        return formAnswer;
    }

    public override void SetAnswer(FormAnswer? answer)
    {
        if (answer == null) return;

        HasValue = answer.BoolValue;
        if (answer?.DateValue != null)
        {
            Day = answer.DateValue.Value.Day.ToString();
            Month = answer.DateValue.Value.Month.ToString();
            Year = answer.DateValue.Value.Year.ToString();
        }
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CurrentFormQuestionType != FormQuestionType.Date)
        {
            yield break;
        }

        var validateField = IsRequired;

        if (IsRequired == false)
        {
            if (HasValue == null)
            {
                yield return new ValidationResult(StaticTextResource.Global_RadioField_SelectOptionError, [nameof(HasValue)]);
            }
            else if (HasValue == true)
            {
                validateField = true;
            }
        }

        if (validateField)
        {
            if (string.IsNullOrWhiteSpace(Day))
            {
                yield return new ValidationResult(StaticTextResource.Global_DateInput_DayRequiredError, [nameof(Day)]);
            }
            else if (!Regex.IsMatch(Day, RegExPatterns.Day))
            {
                yield return new ValidationResult(StaticTextResource.Global_DateInput_DayInvalidError, [nameof(Day)]);
            }

            if (string.IsNullOrWhiteSpace(Month))
            {
                yield return new ValidationResult(StaticTextResource.Global_DateInput_MonthRequiredError, [nameof(Month)]);
            }
            else if (!Regex.IsMatch(Month, RegExPatterns.Month))
            {
                yield return new ValidationResult(StaticTextResource.Global_DateInput_MonthInvalidError, [nameof(Month)]);
            }

            if (string.IsNullOrWhiteSpace(Year))
            {
                yield return new ValidationResult(StaticTextResource.Global_DateInput_YearRequiredError, [nameof(Year)]);
            }
            else if (!Regex.IsMatch(Year, RegExPatterns.Year))
            {
                yield return new ValidationResult(StaticTextResource.Global_DateInput_YearInvalidError, [nameof(Year)]);
            }

            if (!string.IsNullOrWhiteSpace(Day) && !string.IsNullOrWhiteSpace(Month) && !string.IsNullOrWhiteSpace(Year))
            {
                var dateString = $"{Year}-{Month!.PadLeft(2, '0')}-{Day!.PadLeft(2, '0')}";
                if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    yield return new ValidationResult(StaticTextResource.Global_DateInput_DateInvalidError, [nameof(DateString)]);
                }

                if (parsedDate > DateTime.Today)
                {
                    yield return new ValidationResult(StaticTextResource.Global_DateInput_DateInFutureError, [nameof(DateString)]);
                }
            }
        }
    }
}
