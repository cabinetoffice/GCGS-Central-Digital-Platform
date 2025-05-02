namespace CO.CDP.Organisation.WebApi.Model;

public record DeleteConnectedEntityResult
{
    public required bool Success { get; set; }
    public Guid FormGuid { get; set; } = Guid.Empty;
    public Guid SectionGuid { get; set; } = Guid.Empty;
}
