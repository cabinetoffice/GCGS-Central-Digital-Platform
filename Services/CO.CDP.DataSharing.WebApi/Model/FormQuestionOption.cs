using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

internal record FormQuestionOption
{
    [Required] public required Guid Id { get; init; }
    /// <example>"Option 1"</example>
    public required string Value { get; init; }
}