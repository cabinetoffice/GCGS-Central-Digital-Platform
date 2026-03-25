namespace CO.CDP.UserManagement.App.Adapters;

public static class UserManagementAdapterExtensions
{
    public static async Task<Guid?> ResolveOrganisationIdAsync(
        this IUserManagementApiAdapter adapter,
        string organisationSlug,
        CancellationToken ct)
    {
        if (adapter is null) throw new ArgumentNullException(nameof(adapter));
        var org = await adapter.GetOrganisationBySlugAsync(organisationSlug, ct).ConfigureAwait(false);
        return org?.CdpOrganisationGuid;
    }
}