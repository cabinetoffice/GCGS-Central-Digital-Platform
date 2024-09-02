namespace CO.CDP.DataSharing.WebApi.Model;

public record FormAnswer
{
    /// <example>"_Steel01"</example>
    public required string QuestionName { get; init; }

    /// <example>true</example>
    public bool? BoolValue { get; init; }

    /// <example>42</example>
    public double? NumericValue { get; init; }

    /// <example>"2024-04-19:00:00.000Z"</example>
    public DateTime? StartValue { get; init; }

    /// <example>"2024-04-22:00:00.000Z"</example>
    public DateTime? EndValue { get; init; }

    /// <example>"2024-04-22"</example>
    public DateOnly? Date { get; init; }

    /// <example>"Compliance confirmed through third-party audit."</example>
    public string? TextValue { get; init; }

    /// <example>2</example>
    public int? OptionValue { get; init; }
}