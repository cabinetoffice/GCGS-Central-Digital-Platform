using System.Text.Json.Serialization;
using DateValidationType = CO.CDP.Forms.WebApi.Model.Validation.DateValidationType;
using TextValidationType = CO.CDP.Forms.WebApi.Model.Validation.TextValidationType;

namespace CO.CDP.Forms.WebApi.Model;

public record FormQuestionOptions
{
    public List<FormQuestionChoice>? Choices { get; init; } = new();
    public string? ChoiceProviderStrategy { get; init; }
    public string? AnswerFieldName { get; init; }
    public List<FormQuestionGroup>? Groups { get; set; } = new();
    public FormQuestionGrouping? Grouping { get; set; }
    public LayoutOptions? Layout { get; init; }
    public ValidationOptions? Validation { get; init; }
}

public record LayoutOptions
{
    public InputOptions? Input { get; init; }
    public HeadingOptions? Heading { get; init; }
    public ButtonOptions? Button { get; init; }
}

public record InputOptions
{
    public string? CustomYesText { get; init; }
    public string? CustomNoText { get; init; }
    public InputWidthType? Width { get; init; }
    public InputSuffixOptions? Suffix { get; init; }
    public string? CustomCssClasses { get; init; }
}

public record HeadingOptions
{
    public HeadingSize? Size { get; init; }
    public string? BeforeHeadingContent { get; init; }
}

public record ButtonOptions
{
    public string? Text { get; init; }
    public PrimaryButtonStyle? Style { get; init; }
    public string? BeforeButtonContent { get; init; }
    public string? AfterButtonContent { get; init; }
}

public record InputSuffixOptions
{
    public InputSuffixType Type { get; init; }
    public string? Text { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InputSuffixType
{
    GovUkDefault,
    CustomText
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HeadingSize
{
    Small,
    Medium,
    Large,
    ExtraLarge
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PrimaryButtonStyle
{
    Default,
    Start
}

public record ValidationOptions
{
    public DateValidationType? DateValidationType { get; init; }
    public DateTimeOffset? MinDate { get; init; }
    public DateTimeOffset? MaxDate { get; init; }
    public TextValidationType? TextValidationType { get; init; }
}