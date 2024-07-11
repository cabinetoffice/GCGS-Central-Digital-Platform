namespace CO.CDP.EntityVerification.Model;

public class OrganisationRegisteredMessage : EvMessage
{
    public string Scheme { get; set; }
    public string GovIdentifier { get; set; }
    public string Name { get; set;}
}
