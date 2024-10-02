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
    public FormAddress? AddressValue { get; init; }
    public string? JsonValue { get; init; }
}

public record FormAddress
{
    public required string StreetAddress { get; init; }
    public required string Locality { get; init; }
    public string? Region { get; init; }
    public required string PostalCode { get; init; }
    public required string CountryName { get; init; }
    public required string Country { get; init; }
}