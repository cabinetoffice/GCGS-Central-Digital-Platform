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

                var validationApplied = false;
                switch (Options?.Validation?.DateValidationType)
                {
                    case DateValidationType.PastOrTodayOnly:
                        validationApplied = true;
                        if (parsedDate.Date > DateTime.UtcNow.Date)
                        {
                            yield return new ValidationResult(StaticTextResource.Global_DateInput_DateMustBeTodayOrInThePast, [nameof(DateString)]);
                        }
                        break;
                    case DateValidationType.FutureOrTodayOnly:
                        validationApplied = true;
                        if (parsedDate.Date < DateTime.UtcNow.Date)
                        {
                            yield return new ValidationResult(StaticTextResource.Global_DateInput_DateMustBeTodayOrInTheFuture, [nameof(DateString)]);
                        }
                        break;
                    case DateValidationType.MinDate:
                        validationApplied = true;
                        if (Options.Validation.MinDate.HasValue && parsedDate.Date < Options.Validation.MinDate.Value.Date)
                        {
                            yield return new ValidationResult($"The date must be on or after {Options.Validation.MinDate.Value.Date.ToShortDateString()}.", [nameof(DateString)]);
                        }
                        break;
                    case DateValidationType.MaxDate:
                        validationApplied = true;
                        if (Options.Validation.MaxDate.HasValue && parsedDate.Date > Options.Validation.MaxDate.Value.Date)
                        {
                            yield return new ValidationResult(string.Format(StaticTextResource.Global_DateInput_DateMustBeOnOrBefore, Options.Validation.MaxDate.Value.Date.ToShortDateString()), [nameof(DateString)]);
                        }
                        break;
                    case DateValidationType.DateRange:
                        validationApplied = true;
                        if (Options.Validation.MinDate.HasValue && parsedDate.Date < Options.Validation.MinDate.Value.Date || Options.Validation.MaxDate.HasValue && parsedDate.Date > Options.Validation.MaxDate.Value.Date)
                        {
                            yield return new ValidationResult(string.Format(StaticTextResource.Global_DateInput_DateMustBeBetween, Options.Validation.MinDate?.Date.ToShortDateString(), Options.Validation.MaxDate?.Date.ToShortDateString()), [nameof(DateString)]);
                        }
                        break;
                    case DateValidationType.PastOnly:
                        validationApplied = true;
                        if (parsedDate.Date >= DateTime.UtcNow.Date)
                        {
                            yield return new ValidationResult(StaticTextResource.Global_DateInput_DateMustBeInThePast, [nameof(DateString)]);
                        }
                        break;
                    case DateValidationType.FutureOnly:
                        validationApplied = true;
                        if (parsedDate.Date <= DateTime.UtcNow.Date)
                        {
                            yield return new ValidationResult(StaticTextResource.Global_DateInput_DateMustBeInTheFuture, [nameof(DateString)]);
                        }
                        break;
                }

                if (!validationApplied && parsedDate.Date > DateTime.UtcNow.Date)
                {
                    yield return new ValidationResult(StaticTextResource.Global_DateInput_DateMustBeTodayOrInThePast, [nameof(DateString)]);
                }
            }
        }
    }
}
