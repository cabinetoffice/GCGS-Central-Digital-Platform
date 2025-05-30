using System.Text.Json.Serialization;

namespace CO.CDP.Forms.WebApi.Model;

public record FormSection
{
    /// <example>"a3f828e7-93c4-4b44-82d3-ded3ab5f8b0d"</example>
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required FormSectionType Type { get; set; }
    public required bool AllowsMultipleAnswerSets { get; init; }
    public required bool CheckFurtherQuestionsExempted { get; init; }
    public required FormSectionConfiguration Configuration { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FormSectionType
{
    Standard,
    Declaration,
    Exclusions,
    AdditionalSection
}

public record FormSectionConfiguration
{
    public string? SingularSummaryHeadingHint { get; set; }
    public string? SingularSummaryHeading { get; set; }
    public string? PluralSummaryHeadingFormat { get; set; }
    public string? PluralSummaryHeadingHintFormat { get; set; }
    public string? AddAnotherAnswerLabel { get; set; }
    public string? RemoveConfirmationCaption { get; set; }
    public string? RemoveConfirmationHeading { get; set; }
    public string? FurtherQuestionsExemptedHeading { get; set; }
    public string? FurtherQuestionsExemptedHint { get; set; }
    public SummaryRenderFormatter? SummaryRenderFormatter { get; set; }
}

public record SummaryRenderFormatter
{
    public string? KeyExpression { get; set; }
    public List<string> KeyParams { get; set; } = [];
    public ExpressionOperationType KeyExpressionOperation { get; set; }
    public string? ValueExpression { get; set; }
    public List<string> ValueParams { get; set; } = [];
    public ExpressionOperationType ValueExpressionOperation { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExpressionOperationType
{
    StringFormat,
    Equality
}