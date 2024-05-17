using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

internal record QuestionOption
{
    [Required] public required Guid Id { get; init; }
    public string? Value { get; init; }
}