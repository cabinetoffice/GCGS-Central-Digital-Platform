using System.Text.Json.Serialization;

namespace CO.CDP.Forms.WebApi.Model;

public record FormAnswer
{
    public required Guid Id { get; init; }
    public required Guid QuestionId { get; init; }
    public bool? BoolValue { get; init; }
    public double? NumericValue { get; init; }
    public DateTime? DateValue { get; init; }
    public DateTime? StartValue { get; init; }
    public DateTime? EndValue { get; init; }
    public string? TextValue { get; init; }
    public string? OptionValue { get; init; }
}