namespace CO.CDP.DataSharing.WebApi.Model;

public record FormSection
{
    /// <example>"1"</example>
    public int Id { get; set; }

    /// <example>"3b3a269a-c1fa-4bfa-8892-7c6a9aef03bb"</example>
    public required Guid Guid { get; set; }

    /// <example>"Some Section"</example>
    public required string Title { get; set; }

    /// <example>"Standard"</example>
    public required FormSectionType Type { get; set; }

    /// <example>"1"</example>
    public required int FormId { get; set; }

    public required Form Form { get; set; }

    public required ICollection<FormQuestion> Questions { get; set; } = [];

    public required bool AllowsMultipleAnswerSets { get; set; }

    /// <example>"AddAnotherAnswerLabel"</example>
    public required FormSectionConfiguration Configuration;
}