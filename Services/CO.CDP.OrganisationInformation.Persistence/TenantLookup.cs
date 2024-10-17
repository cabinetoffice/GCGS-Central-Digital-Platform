namespace CO.CDP.OrganisationInformation.Persistence;


public class TenantLookup
{
    public required PersonUser User { get; init; }
    public required List<Tenant> Tenants { get; init; }

    public class PersonUser
    {
        public required string Name { get; init; }
        public required string Urn { get; init; }
        public required string Email { get; init; }
    }

    public class Tenant
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public required List<Organisation> Organisations { get; init; }
    }

    public class Organisation
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public required List<PartyRole> Roles { get; init; }
        public required List<PartyRole> PendingRoles { get; init; }
        public required List<string> Scopes { get; init; }
        public DateTimeOffset? ApprovedOn{get;init;}
    }
}