using CO.CDP.EntityFrameworkCore.Timestamps;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

[Index(nameof(Guid), IsUnique = true)]
public class PersonInvite : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required int OrganisationId { get; set; }
    public Organisation? Organisation { get; set; }

    public Person? Person { get; set; }

    public required List<string> Scopes { get; set; } = [];
    public DateTimeOffset InviteSentOn { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }

    public DateTimeOffset? ExpiresOn { get; set; }
}