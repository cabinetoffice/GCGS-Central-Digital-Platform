namespace CO.CDP.DataSharing.WebApi.Model;

public record FormAnswerSet
{
    /// <example>"3b3a269a-c1fa-4bfa-8892-7c6a9aef03bb"</example>
    public required Guid Id { get; set; }

    public required string SectionName { get; set; }

    public required ICollection<FormAnswer> Answers { get; set; } = [];

    /// <example>"47e6a363-11c0-4cf4-bce6-dea03034e4bb"</example>
    public Guid? OrganisationId { get; set; }
}