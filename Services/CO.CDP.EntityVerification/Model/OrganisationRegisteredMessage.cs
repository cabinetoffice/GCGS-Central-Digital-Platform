namespace CO.CDP.EntityVerification.Model;

public class OrganisationRegisteredMessage : EvMessage
{
    public required string Scheme { get; set; }
    public required string GovIdentifier { get; set; }
    public required string Name { get; set;}
}
