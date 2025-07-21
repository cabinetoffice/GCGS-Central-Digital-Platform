using System.ComponentModel.DataAnnotations;

namespace CO.CDP.RegisterOfCommercialTools.App.Validation;

public class DateComponentValidationAttribute(string dateFieldName) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var currentPropertyName = validationContext.MemberName;
        var componentType = currentPropertyName?.Replace(dateFieldName, "").ToLower();

        var dayProperty = validationContext.ObjectType.GetProperty($"{dateFieldName}Day");
        var monthProperty = validationContext.ObjectType.GetProperty($"{dateFieldName}Month");
        var yearProperty = validationContext.ObjectType.GetProperty($"{dateFieldName}Year");

        var day = dayProperty?.GetValue(validationContext.ObjectInstance)?.ToString();
        var month = monthProperty?.GetValue(validationContext.ObjectInstance)?.ToString();
        var year = yearProperty?.GetValue(validationContext.ObjectInstance)?.ToString();

        // If all components are empty, it's valid (optional field)
        if (string.IsNullOrWhiteSpace(day) && string.IsNullOrWhiteSpace(month) && string.IsNullOrWhiteSpace(year))
        {
            return ValidationResult.Success;
        }

        var hasAnyValue = !string.IsNullOrWhiteSpace(day) || !string.IsNullOrWhiteSpace(month) ||
                          !string.IsNullOrWhiteSpace(year);

        if (hasAnyValue)
        {
            var missingComponents = new List<string>();

            if (string.IsNullOrWhiteSpace(day)) missingComponents.Add("day");
            if (string.IsNullOrWhiteSpace(month)) missingComponents.Add("month");
            if (string.IsNullOrWhiteSpace(year)) missingComponents.Add("year");

            switch (componentType)
            {
                case "day" when string.IsNullOrWhiteSpace(day) && missingComponents.Count != 0:
                {
                    var fieldDisplayName = GetFieldDisplayName(dateFieldName);
                    var errorMessage = GenerateErrorMessage(fieldDisplayName, missingComponents);
                    return new ValidationResult(errorMessage, [$"{dateFieldName}Day"]);
                }
                case "month" when string.IsNullOrWhiteSpace(month) && missingComponents.Count != 0:
                {
                    var fieldDisplayName = GetFieldDisplayName(dateFieldName);
                    var errorMessage = GenerateErrorMessage(fieldDisplayName, missingComponents);
                    return new ValidationResult(errorMessage, [$"{dateFieldName}Month"]);
                }
                case "year" when string.IsNullOrWhiteSpace(year) && missingComponents.Count != 0:
                {
                    var fieldDisplayName = GetFieldDisplayName(dateFieldName);
                    var errorMessage = GenerateErrorMessage(fieldDisplayName, missingComponents);
                    return new ValidationResult(errorMessage, [$"{dateFieldName}Year"]);
                }
            }
        }

        if (componentType == "day" && !string.IsNullOrWhiteSpace(day) &&
            (!int.TryParse(day, out var dayValue) || dayValue < 1 || dayValue > 31))
        {
            return new ValidationResult($"{GetFieldDisplayName(dateFieldName)} must have a valid day",
                [$"{dateFieldName}Day"]);
        }

        if (componentType == "month" && !string.IsNullOrWhiteSpace(month) &&
            (!int.TryParse(month, out var monthValue) || monthValue < 1 || monthValue > 12))
        {
            return new ValidationResult($"{GetFieldDisplayName(dateFieldName)} must have a valid month",
                [$"{dateFieldName}Month"]);
        }

        if (componentType == "year" && !string.IsNullOrWhiteSpace(year) &&
            (!int.TryParse(year, out _) || year.Length != 4))
        {
            return new ValidationResult($"{GetFieldDisplayName(dateFieldName)} year must include 4 numbers",
                [$"{dateFieldName}Year"]);
        }

        if (componentType != "day" || string.IsNullOrWhiteSpace(day) || string.IsNullOrWhiteSpace(month) ||
            string.IsNullOrWhiteSpace(year) ||
            !int.TryParse(day, out var dayInt) || !int.TryParse(month, out var monthInt) ||
            !int.TryParse(year, out var yearInt)) return ValidationResult.Success;
        return !DateTime.TryParse($"{yearInt}-{monthInt:D2}-{dayInt:D2}", out _)
            ? new ValidationResult($"{GetFieldDisplayName(dateFieldName)} must be a real date", [$"{dateFieldName}Day"])
            : ValidationResult.Success;
    }

    private static string GenerateErrorMessage(string fieldDisplayName, List<string> missingComponents)
    {
        return missingComponents.Count switch
        {
            1 => $"{fieldDisplayName} must include a {missingComponents[0]}",
            2 => $"{fieldDisplayName} must include a {string.Join(" and ", missingComponents)}",
            _ =>
                $"{fieldDisplayName} must include a {string.Join(", ", missingComponents.Take(missingComponents.Count - 1))} and {missingComponents.Last()}"
        };
    }

    private static string GetFieldDisplayName(string fieldName)
    {
        return fieldName switch
        {
            "SubmissionDeadlineFrom" => "Submission deadline from",
            "SubmissionDeadlineTo" => "Submission deadline to",
            "ContractStartDateFrom" => "Contract start date from",
            "ContractStartDateTo" => "Contract start date to",
            "ContractEndDateFrom" => "Contract end date from",
            "ContractEndDateTo" => "Contract end date to",
            _ => fieldName
        };
    }
}