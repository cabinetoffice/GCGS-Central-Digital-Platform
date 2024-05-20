namespace CO.CDP.DataSharing.WebApi.Model;

internal record FormAnswer
{
    public string? QuestionName { get; init; }
    public bool? BoolValue { get; init; }
    public double? NumericValue { get; init; }
    public DateTime? StartValue { get; init; }
    public DateTime? EndValue { get; init; }
    public string? TextValue { get; init; }
    public int? OptionValue { get; init; }
}