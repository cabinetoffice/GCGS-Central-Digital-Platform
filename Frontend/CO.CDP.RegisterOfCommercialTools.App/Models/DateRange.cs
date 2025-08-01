using System.ComponentModel.DataAnnotations;

namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class DateRange(string fieldDisplayName) : IValidatableObject
{
    public DateComponent From { get; init; } = new($"{fieldDisplayName} from");
    public DateComponent To { get; init; } = new($"{fieldDisplayName} to");

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (From.Value.HasValue && To.Value.HasValue && To.Value < From.Value)
        {
            yield return new ValidationResult(
                $"{fieldDisplayName} to date must be after from date",
                [nameof(To)]
            );
        }
    }
}