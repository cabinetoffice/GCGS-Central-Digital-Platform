namespace CO.CDP.OrganisationInformation.Persistence;


public class ConnectedEntityLookup
{
    public required string Name { get; init; }
    public required Guid EntityId { get; init; }
    public required ConnectedEntityType EntityType { get; set; }
    public DateTimeOffset? EndDate { get; init; }
}