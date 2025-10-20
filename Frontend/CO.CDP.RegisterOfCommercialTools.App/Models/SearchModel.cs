using System.ComponentModel.DataAnnotations;
using CO.CDP.RegisterOfCommercialTools.App.Validation;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class SearchModel : IValidatableObject
{
    [FromQuery(Name = "q")] public string? Keywords { get; set; }
    [FromQuery(Name = "sort")] public string? SortOrder { get; set; }
    [FromQuery(Name = "filter_frameworks")] public bool FilterFrameworks { get; set; }
    [FromQuery(Name = "open_frameworks")] public bool IsOpenFrameworks { get; set; }
    [FromQuery(Name = "filter_markets")] public bool FilterDynamicMarkets { get; set; }
    [FromQuery(Name = "utilities_only")] public bool IsUtilitiesOnly { get; set; }

    [FromQuery(Name = "award")] public List<string> AwardMethod { get; set; } = [];

    [FromQuery(Name = "fee_min")]
    [Range(0, 100, ErrorMessage = "Enter a value between 0 and 100")]
    public decimal? FeeMin { get; set; }

    [FromQuery(Name = "fee_max")]
    [Range(0, 100, ErrorMessage = "Enter a value between 0 and 100")]
    [DecimalRange("FeeMin", ErrorMessage = "To must be more than from")]
    public decimal? FeeMax { get; set; }

    [FromQuery(Name = "no_fees")] public string? NoFees { get; set; }
    [FromQuery(Name = "status")] public List<string> Status { get; set; } = [];
    [FromQuery(Name = "usage")] public string? ContractingAuthorityUsage { get; set; }

    [FromQuery(Name = "cpv")] public List<string> CpvCodes { get; set; } = [];
    [FromQuery(Name = "location")] public List<string> LocationCodes { get; set; } = [];

    [FromQuery(Name = "sub_from_day")] public string? SubmissionDeadlineFromDay { get; set; }
    [FromQuery(Name = "sub_from_month")] public string? SubmissionDeadlineFromMonth { get; set; }
    [FromQuery(Name = "sub_from_year")] public string? SubmissionDeadlineFromYear { get; set; }

    [FromQuery(Name = "sub_to_day")] public string? SubmissionDeadlineToDay { get; set; }
    [FromQuery(Name = "sub_to_month")] public string? SubmissionDeadlineToMonth { get; set; }
    [FromQuery(Name = "sub_to_year")] public string? SubmissionDeadlineToYear { get; set; }

    [FromQuery(Name = "start_from_day")] public string? ContractStartDateFromDay { get; set; }
    [FromQuery(Name = "start_from_month")] public string? ContractStartDateFromMonth { get; set; }
    [FromQuery(Name = "start_from_year")] public string? ContractStartDateFromYear { get; set; }

    [FromQuery(Name = "start_to_day")] public string? ContractStartDateToDay { get; set; }
    [FromQuery(Name = "start_to_month")] public string? ContractStartDateToMonth { get; set; }
    [FromQuery(Name = "start_to_year")] public string? ContractStartDateToYear { get; set; }

    [FromQuery(Name = "end_from_day")] public string? ContractEndDateFromDay { get; set; }
    [FromQuery(Name = "end_from_month")] public string? ContractEndDateFromMonth { get; set; }
    [FromQuery(Name = "end_from_year")] public string? ContractEndDateFromYear { get; set; }

    [FromQuery(Name = "end_to_day")] public string? ContractEndDateToDay { get; set; }
    [FromQuery(Name = "end_to_month")] public string? ContractEndDateToMonth { get; set; }
    [FromQuery(Name = "end_to_year")] public string? ContractEndDateToYear { get; set; }

    public DateOnly? SubmissionDeadlineFrom => TryParseDate(SubmissionDeadlineFromDay, SubmissionDeadlineFromMonth, SubmissionDeadlineFromYear);
    public DateOnly? SubmissionDeadlineTo => TryParseDate(SubmissionDeadlineToDay, SubmissionDeadlineToMonth, SubmissionDeadlineToYear);
    public DateOnly? ContractStartDateFrom => TryParseDate(ContractStartDateFromDay, ContractStartDateFromMonth, ContractStartDateFromYear);
    public DateOnly? ContractStartDateTo => TryParseDate(ContractStartDateToDay, ContractStartDateToMonth, ContractStartDateToYear);
    public DateOnly? ContractEndDateFrom => TryParseDate(ContractEndDateFromDay, ContractEndDateFromMonth, ContractEndDateFromYear);
    public DateOnly? ContractEndDateTo => TryParseDate(ContractEndDateToDay, ContractEndDateToMonth, ContractEndDateToYear);

    private static DateOnly? TryParseDate(string? day, string? month, string? year)
    {
        if (string.IsNullOrWhiteSpace(day) && string.IsNullOrWhiteSpace(month) && string.IsNullOrWhiteSpace(year))
            return null;

        if (!string.IsNullOrWhiteSpace(day) && !string.IsNullOrWhiteSpace(month) && !string.IsNullOrWhiteSpace(year) &&
            int.TryParse(day, out var d) && int.TryParse(month, out var m) && int.TryParse(year, out var y) &&
            DateTime.TryParse($"{y}-{m:D2}-{d:D2}", out var date))
        {
            return DateOnly.FromDateTime(date);
        }

        return null;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!FilterFrameworks)
        {
            IsOpenFrameworks = false;
        }

        if (!FilterDynamicMarkets)
        {
            IsUtilitiesOnly = false;
        }

        if (!string.IsNullOrEmpty(NoFees))
        {
            var feeFromHasValue = FeeMin.HasValue;
            var feeToHasValue = FeeMax.HasValue;

            switch (feeFromHasValue)
            {
                case true when feeToHasValue:
                    yield return new ValidationResult(
                        "Fee from and to cannot be provided when 'No fees' is selected",
                        [nameof(FeeMin), nameof(FeeMax)]
                    );
                    break;
                case true:
                    yield return new ValidationResult(
                        "Fee from cannot be provided when 'No fees' is selected",
                        [nameof(FeeMin)]
                    );
                    break;
                default:
                {
                    if (feeToHasValue)
                    {
                        yield return new ValidationResult(
                            "Fee to cannot be provided when 'No fees' is selected",
                            [nameof(FeeMax)]
                        );
                    }

                    break;
                }
            }
        }

        var submissionFromErrors = ValidateDateComponents(SubmissionDeadlineFromDay, SubmissionDeadlineFromMonth, SubmissionDeadlineFromYear, "Submission deadline from");
        foreach (var error in submissionFromErrors)
        {
            yield return new ValidationResult(error.Message, error.Members);
        }

        var submissionToErrors = ValidateDateComponents(SubmissionDeadlineToDay, SubmissionDeadlineToMonth, SubmissionDeadlineToYear, "Submission deadline to");
        foreach (var error in submissionToErrors)
        {
            yield return new ValidationResult(error.Message, error.Members);
        }

        if (SubmissionDeadlineFrom.HasValue && SubmissionDeadlineTo.HasValue && SubmissionDeadlineTo < SubmissionDeadlineFrom)
        {
            yield return new ValidationResult(
                DateValidationMessages.GetDateRangeMessage("Submission deadline"),
                [nameof(SubmissionDeadlineToDay), nameof(SubmissionDeadlineToMonth), nameof(SubmissionDeadlineToYear)]);
        }

        var contractStartFromErrors = ValidateDateComponents(ContractStartDateFromDay, ContractStartDateFromMonth, ContractStartDateFromYear, "Contract start date from");
        foreach (var error in contractStartFromErrors)
        {
            yield return new ValidationResult(error.Message, error.Members);
        }

        var contractStartToErrors = ValidateDateComponents(ContractStartDateToDay, ContractStartDateToMonth, ContractStartDateToYear, "Contract start date to");
        foreach (var error in contractStartToErrors)
        {
            yield return new ValidationResult(error.Message, error.Members);
        }

        if (ContractStartDateFrom.HasValue && ContractStartDateTo.HasValue && ContractStartDateTo < ContractStartDateFrom)
        {
            yield return new ValidationResult(
                DateValidationMessages.GetDateRangeMessage("Contract start date"),
                [nameof(ContractStartDateToDay), nameof(ContractStartDateToMonth), nameof(ContractStartDateToYear)]);
        }
    }

    private IEnumerable<(string Message, string[] Members)> ValidateDateComponents(string? day, string? month, string? year, string fieldName)
    {
        var hasDay = !string.IsNullOrWhiteSpace(day);
        var hasMonth = !string.IsNullOrWhiteSpace(month);
        var hasYear = !string.IsNullOrWhiteSpace(year);

        if (!hasDay && !hasMonth && !hasYear)
            yield break;

        var propertyPrefix = GetPropertyPrefix(fieldName);
        var missing = new List<DateComponentType>();
        if (!hasDay) missing.Add(DateComponentType.Day);
        if (!hasMonth) missing.Add(DateComponentType.Month);
        if (!hasYear) missing.Add(DateComponentType.Year);

        if (missing.Any())
        {
            var message = DateValidationMessages.GetMissingComponentsMessage(fieldName, missing);
            var members = missing.Select(m => GetMemberName(propertyPrefix, m)).ToArray();
            yield return (message, members);
            yield break;
        }

        var dayValid = int.TryParse(day, out var dayInt) && dayInt >= 1 && dayInt <= 31;
        var monthValid = int.TryParse(month, out var monthInt) && monthInt >= 1 && monthInt <= 12;
        var yearValid = int.TryParse(year, out _) && year.Length == 4;

        if (!dayValid)
        {
            yield return (DateValidationMessages.GetInvalidDayMessage(fieldName), [GetMemberName(propertyPrefix, DateComponentType.Day)]);
        }

        if (!monthValid)
        {
            yield return (DateValidationMessages.GetInvalidMonthMessage(fieldName), [GetMemberName(propertyPrefix, DateComponentType.Month)]);
        }

        if (!yearValid)
        {
            yield return (DateValidationMessages.GetInvalidYearMessage(fieldName), [GetMemberName(propertyPrefix, DateComponentType.Year)]);
        }

        if (dayValid && monthValid && yearValid && !DateTime.TryParse($"{year}-{monthInt:D2}-{dayInt:D2}", out _))
        {
            yield return (DateValidationMessages.GetInvalidDateMessage(fieldName), [GetMemberName(propertyPrefix, DateComponentType.Day)]);
        }
    }

    private static string GetPropertyPrefix(string fieldName) => fieldName switch
    {
        "Submission deadline from" => nameof(SubmissionDeadlineFrom),
        "Submission deadline to" => nameof(SubmissionDeadlineTo),
        "Contract start date from" => nameof(ContractStartDateFrom),
        "Contract start date to" => nameof(ContractStartDateTo),
        _ => throw new ArgumentException($"Unknown field name: {fieldName}")
    };

    private string GetMemberName(string propertyPrefix, DateComponentType component) => component switch
    {
        DateComponentType.Day when propertyPrefix == nameof(SubmissionDeadlineFrom) => nameof(SubmissionDeadlineFromDay),
        DateComponentType.Month when propertyPrefix == nameof(SubmissionDeadlineFrom) => nameof(SubmissionDeadlineFromMonth),
        DateComponentType.Year when propertyPrefix == nameof(SubmissionDeadlineFrom) => nameof(SubmissionDeadlineFromYear),
        DateComponentType.Day when propertyPrefix == nameof(SubmissionDeadlineTo) => nameof(SubmissionDeadlineToDay),
        DateComponentType.Month when propertyPrefix == nameof(SubmissionDeadlineTo) => nameof(SubmissionDeadlineToMonth),
        DateComponentType.Year when propertyPrefix == nameof(SubmissionDeadlineTo) => nameof(SubmissionDeadlineToYear),
        DateComponentType.Day when propertyPrefix == nameof(ContractStartDateFrom) => nameof(ContractStartDateFromDay),
        DateComponentType.Month when propertyPrefix == nameof(ContractStartDateFrom) => nameof(ContractStartDateFromMonth),
        DateComponentType.Year when propertyPrefix == nameof(ContractStartDateFrom) => nameof(ContractStartDateFromYear),
        DateComponentType.Day when propertyPrefix == nameof(ContractStartDateTo) => nameof(ContractStartDateToDay),
        DateComponentType.Month when propertyPrefix == nameof(ContractStartDateTo) => nameof(ContractStartDateToMonth),
        DateComponentType.Year when propertyPrefix == nameof(ContractStartDateTo) => nameof(ContractStartDateToYear),
        _ => throw new ArgumentException($"Unknown combination: {propertyPrefix}, {component}")
    };

    private enum DateComponentType
    {
        Day,
        Month,
        Year
    }

    private static class DateValidationMessages
    {

        public static string GetMissingComponentsMessage(string fieldName, List<DateComponentType> missing)
        {
            var components = missing.Select(m => m.ToString().ToLower()).ToList();
            return components.Count switch
            {
                1 => $"{fieldName} must include a {components[0]}",
                2 => $"{fieldName} must include a {string.Join(" and ", components)}",
                _ => $"{fieldName} must include a {string.Join(", ", components.Take(components.Count - 1))} and {components.Last()}"
            };
        }

        public static string GetInvalidDayMessage(string fieldName) => $"{fieldName} must have a valid day";
        public static string GetInvalidMonthMessage(string fieldName) => $"{fieldName} must have a valid month";
        public static string GetInvalidYearMessage(string fieldName) => $"{fieldName} year must include 4 numbers";
        public static string GetInvalidDateMessage(string fieldName) => $"{fieldName} must be a real date";
        public static string GetDateRangeMessage(string fieldName) => $"{fieldName} to date must be after from date";
    }
}