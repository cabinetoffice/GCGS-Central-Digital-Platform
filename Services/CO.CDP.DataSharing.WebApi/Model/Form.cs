using System.Text.Json.Serialization;

namespace CO.CDP.DataSharing.WebApi.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
internal enum FormSubmissionState
{
    Draft,
    Submitted
}

internal record Form
{
    /// <example>"Standard Questions"</example>
    public required string Name { get; init; }

    /// <example>"Submitted"</example>
    public required FormSubmissionState SubmissionState { get; init; }

    /// <example>"2024-04-28T21:53:26.377Z"</example>
    public required DateTime SubmittedAt { get; init; }

    /// <example>"5a360be7-e1d3-4214-9f72-0e1d6b57b85d"</example>
    public required Guid OrganisationId { get; init; }

    /// <example>"3b3a269a-c1fa-4bfa-8892-7c6a9aef03bb"</example>
    public required Guid FormId { get; init; }

    /// <example>"20240429"</example>
    public required string FormVersionId { get; init; }

    /// <example>true</example>
    public required bool IsRequired { get; init; }

    /// <example>"AGMT-2024-XYZ"</example>
    public required string BookingReference { get; init; }

    /// <example>0</example>
    public required int Scope { get; init; }

    /// <example>0</example>
    public required int Type { get; init; }
}