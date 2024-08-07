using System.Text.Json.Serialization;

namespace CO.CDP.Forms.WebApi.Model;

public record FormQuestion
{
    /// <example>"fcdb1f2d-5e4f-4b8d-8fd1-f35c8d3f8f8f"</example>
    public required Guid Id { get; init; }
    public Guid? NextQuestion { get; init; }
    public Guid? NextQuestionAlternative { get; init; }
    public required FormQuestionType Type { get; init; }
    public required bool IsRequired { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public string? Caption { get; init; }
    public required FormQuestionOptions Options { get; init; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FormQuestionType
{
    NoInput,
    Text,
    FileUpload,
    YesOrNo,
    SingleChoice,
    MultipleChoice,
    CheckYourAnswers,
    Date,
    CheckBox,
    Address
}