namespace CO.CDP.Organisation.WebApi.Model;

public record MouSignature
{
    public required Guid Id { get; init; }
    public required Mou Mou { get; init; } 
    public required Person Person { get; init; }
    public required string JobTitle { get; init; }
    public required DateTimeOffset SignatureOn { get; set; }
}