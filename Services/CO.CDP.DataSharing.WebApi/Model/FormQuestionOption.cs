using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

public record FormQuestionOption
{
    [Required] public required Guid Id { get; init; }
    /// <example>"3fa85f64-5717-4562-b3fc-2c963f66afa6"</example>
    public required string Value { get; init; }
}