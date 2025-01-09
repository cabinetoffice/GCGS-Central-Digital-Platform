namespace CO.CDP.Organisation.WebApi.Model;

public record Mou
{
    public required Guid Id { get; init; }    
    public required string FilePath { get; init; }
    public required DateTimeOffset CreatedOn { get; set; }
}