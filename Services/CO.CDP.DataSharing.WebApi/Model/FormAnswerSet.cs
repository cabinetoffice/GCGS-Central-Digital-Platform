namespace CO.CDP.DataSharing.WebApi.Model;

public record FormAnswerSet
{
    /// <example>"1"</example>
    public int Id { get; set; }

    /// <example>"3b3a269a-c1fa-4bfa-8892-7c6a9aef03bb"</example>
    public required Guid Guid { get; set; }

    /// <example>"1"</example>
    public required int SharedConsentId { get; set; }

    public required SharedConsent SharedConsent { get; set; }

    /// <example>"1"</example>
    public required int SectionId { get; set; }

    public required FormSection Section { get; init; }

    public required ICollection<FormAnswer> Answers { get; set; } = [];

    /// <example>"false"</example>
    public bool Deleted { get; set; } = false;
}