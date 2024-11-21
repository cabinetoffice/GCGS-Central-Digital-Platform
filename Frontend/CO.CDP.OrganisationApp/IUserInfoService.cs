using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Tenant.WebApiClient;

namespace CO.CDP.OrganisationApp;

public interface IUserInfoService
{
    public Task<UserInfo> GetUserInfo();
    public Guid? GetOrganisationId();
    public Task<bool> IsViewer();
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

    public bool IsViewerOf(Guid? organisationId)
    {
        var organisationUserScopes = Organisations
            .Where(o => o.Id == organisationId)
            .SelectMany(o => o.Scopes)
            .ToList();

        return organisationUserScopes.Contains(OrganisationPersonScopes.Viewer) ||
               (organisationUserScopes.Count == 0 && Scopes.Contains(PersonScopes.SupportAdmin));
    }
}

public record UserOrganisationInfo
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public ICollection<PartyRole> Roles { get; init; } = [];
    public ICollection<PartyRole> PendingRoles { get; init; } = [];
    public ICollection<string> Scopes { get; init; } = [];
}