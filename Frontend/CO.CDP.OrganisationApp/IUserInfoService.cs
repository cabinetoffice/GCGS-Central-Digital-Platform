using CO.CDP.Tenant.WebApiClient;

namespace CO.CDP.OrganisationApp;

public interface IUserInfoService
{
    public Task<UserInfo> GetUserInfo();
    public Guid? GetOrganisationId();
    public Task<bool> IsViewer();
    public Task<bool> IsAdmin();
    public Task<bool> HasOrganisations();
    bool IsAuthenticated();
}

public record UserInfo
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public ICollection<string> Scopes { get; init; } = [];
    public ICollection<UserOrganisationInfo> Organisations { get; init; } = [];

    public bool HasOrganisations()
    {
        return Organisations.Count > 0;
    }

    public ICollection<string> OrganisationScopes(Guid? organisationId)
    {
        return Organisations
            .Where(o => o.Id == organisationId)
            .SelectMany(o => o.Scopes).ToList();
    }
}

public record UserOrganisationInfo
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public ICollection<PartyRole> Roles { get; init; } = [];
    public ICollection<PartyRole> PendingRoles { get; init; } = [];
    public ICollection<string> Scopes { get; init; } = [];
    public OrganisationType Type { get; init; }
}
