namespace CO.CDP.OrganisationInformation;
public class OrganisationQualification
{
    public int Id { get; set; }
    public string? AwardingByPersonOrBodyName { get; set; }
    public DateTimeOffset DateAwarded { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}
