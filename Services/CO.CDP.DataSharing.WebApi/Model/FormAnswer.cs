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
    public DateOnly? DateValue { get; init; }

    /// <example>"Compliance confirmed through third-party audit."</example>
    public string? TextValue { get; init; }

    /// <example>["option-id-1"]</example>
    public List<string> OptionValue { get; init; } = [];

    /// <example>{"id": "00000000-0000-0000-0000-000000000000", "type": "organisation"}</example>
    public Dictionary<string, object>? JsonValue { get; init; }
}