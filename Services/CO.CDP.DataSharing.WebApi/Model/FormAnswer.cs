namespace CO.CDP.DataSharing.WebApi.Model;

public record FormAnswer
{
    /// <example>"_Steel01"</example>
    public required string QuestionName { get; init; }

    /// <example>true</example>
    public bool? BoolValue { get; init; }

    /// <example>42</example>
    public double? NumericValue { get; init; }

    /// <example>"2024-04-19T00:00.000Z"</example>
    public DateTimeOffset? StartValue { get; init; }

    /// <example>"2024-04-22T00:00.000Z"</example>
    public DateTimeOffset? EndValue { get; init; }

    /// <example>"2024-04-22"</example>
    public DateOnly? DateValue { get; init; }

    /// <example>"Compliance confirmed through third-party audit."</example>
    public string? TextValue { get; set; }

    /// <example>["option-id-1"]</example>
    public List<string> OptionValue { get; init; } = [];

    /// <example>{"id": "00000000-0000-0000-0000-000000000000", "type": "organisation OR connected-entity"}</example>
    public Dictionary<string, object>? JsonValue { get; init; }

    /// <example>https://data-sharing.api.net/data/share/share_code/documents/document.pdf</example>
    public Uri? DocumentUri { get; set; }
}