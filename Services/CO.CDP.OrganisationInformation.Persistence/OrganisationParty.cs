using CO.CDP.EntityFrameworkCore.Timestamps;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using System.ComponentModel.DataAnnotations.Schema;

namespace CO.CDP.OrganisationInformation.Persistence;

public class OrganisationParty : IEntityDate
{
    public int Id { get; set; }

    [ForeignKey(nameof(ParentOrganisation))]
    public required int ParentOrganisationId { get; set; }

    [ForeignKey(nameof(ChildOrganisation))]
    public required int ChildOrganisationId { get; set; }

    public required OrganisationRelationship OrganisationRelationship { get; set; }

    [ForeignKey(nameof(SharedConsent))]
    public int? SharedConsentId { get; set; }

    public Organisation? ParentOrganisation { get; set; }
    public Organisation? ChildOrganisation { get; set; }
    public SharedConsent? SharedConsent { get; set; }

    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}

public enum OrganisationRelationship
{
    Consortium
}