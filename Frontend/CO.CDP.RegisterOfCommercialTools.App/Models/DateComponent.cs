using System.ComponentModel.DataAnnotations;


namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class DateComponent : IValidatableObject
{
    private readonly string _displayName;

    public DateComponent(string displayName)
    {
        _displayName = displayName;
    }

    public string? Day { get; set; }

    public string? Month { get; set; }

    public string? Year { get; set; }

    public string DisplayName => _displayName;

    public DateOnly? Value => TryParseDate();

    public bool HasValue => !string.IsNullOrWhiteSpace(Day) || !string.IsNullOrWhiteSpace(Month) || !string.IsNullOrWhiteSpace(Year);

    public bool IsComplete => !string.IsNullOrWhiteSpace(Day) && !string.IsNullOrWhiteSpace(Month) && !string.IsNullOrWhiteSpace(Year);

    private DateOnly? TryParseDate()
    {
        if (!IsComplete) return null;

        if (int.TryParse(Day, out var d) && int.TryParse(Month, out var m) && int.TryParse(Year, out var y))
        {
            if (DateTime.TryParse($"{y}-{m:D2}-{d:D2}", out var date))
                return DateOnly.FromDateTime(date);
        }

        return null;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!HasValue)
        {
            yield break; // If no values are entered, it's valid (optional field)
        }

        var missingComponents = new List<string>();

        if (string.IsNullOrWhiteSpace(Day)) missingComponents.Add("day");
        if (string.IsNullOrWhiteSpace(Month)) missingComponents.Add("month");
        if (string.IsNullOrWhiteSpace(Year)) missingComponents.Add("year");

        if (missingComponents.Any())
        {
            var errorMessage = GenerateMissingComponentsErrorMessage(DisplayName, missingComponents);
            yield return new ValidationResult(errorMessage, missingComponents.Select(c => c == "day" ? nameof(Day) : c == "month" ? nameof(Month) : nameof(Year)).ToArray());
            yield break;
        }

        bool dayParsed = int.TryParse(Day, out var dayInt);
        bool monthParsed = int.TryParse(Month, out var monthInt);
        bool yearParsed = int.TryParse(Year, out var yearInt);

        if (!dayParsed || dayInt < 1 || dayInt > 31)
        {
            yield return new ValidationResult($"{DisplayName} must have a valid day", new[] { nameof(Day) });
            yield break;
        }

        if (!monthParsed || monthInt < 1 || monthInt > 12)
        {
            yield return new ValidationResult($"{DisplayName} must have a valid month", new[] { nameof(Month) });
            yield break;
        }

        if (!yearParsed || Year?.Length != 4)
        {
            yield return new ValidationResult($"{DisplayName} year must include 4 numbers", new[] { nameof(Year) });
            yield break;
        }

        if (dayParsed && monthParsed && yearParsed && IsComplete && !TryParseDate().HasValue)
        {
            yield return new ValidationResult($"{DisplayName} must be a real date", new[] { nameof(Day), nameof(Month), nameof(Year) });
        }
    }

    private static string GenerateMissingComponentsErrorMessage(string fieldDisplayName, List<string> missingComponents)
    {
        return missingComponents.Count switch
        {
            1 => $"{fieldDisplayName} must include a {missingComponents[0]}",
            2 => $"{fieldDisplayName} must include a {string.Join(" and ", missingComponents)}",
            _ => $"{fieldDisplayName} must include a {string.Join(", ", missingComponents.Take(missingComponents.Count - 1))} and {missingComponents.Last()}"
        };
    }
}