using CO.CDP.ApplicationRegistry.App.Models;
using ApiClient = CO.CDP.ApplicationRegistry.WebApiClient;

namespace CO.CDP.ApplicationRegistry.App.Services;

public sealed class UserService(ApiClient.ApplicationRegistryClient apiClient) : IUserService
{
    public async Task<UsersViewModel?> GetUsersViewModelAsync(
        string organisationSlug,
        string? selectedRole = null,
        string? selectedApplication = null,
        string? searchTerm = null,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var usersResponse = await apiClient.UsersAll2Async(org.Id, ct);

            var users = usersResponse
                .Select(user => new UserSummaryViewModel(
                    UserId: user.UserPrincipalId,
                    Name: user.UserPrincipalId,
                    Email: user.UserPrincipalId,
                    OrganisationRole: user.OrganisationRole,
                    Status: user.Status,
                    ApplicationAccess: user.ApplicationAssignments?
                        .Where(assignment => assignment.Application != null)
                        .Select(assignment => new UserApplicationAccessViewModel(
                            ApplicationName: assignment.Application!.Name,
                            ApplicationSlug: assignment.Application!.ClientId,
                            RoleName: assignment.Roles == null
                                ? string.Empty
                                : string.Join(", ", assignment.Roles.Select(role => role.Name))))
                        .ToList() ?? []))
                .ToList();

            var filteredUsers = users
                .Where(user => string.IsNullOrWhiteSpace(selectedRole) ||
                               user.OrganisationRole.ToString().Equals(selectedRole, StringComparison.OrdinalIgnoreCase))
                .Where(user => string.IsNullOrWhiteSpace(selectedApplication) ||
                               user.ApplicationAccess.Any(app =>
                                   app.ApplicationSlug.Equals(selectedApplication, StringComparison.OrdinalIgnoreCase)))
                .Where(user => string.IsNullOrWhiteSpace(searchTerm) ||
                               user.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                               user.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return new UsersViewModel(
                OrganisationName: org.Name,
                OrganisationSlug: org.Slug,
                Users: filteredUsers,
                SelectedRole: selectedRole,
                SelectedApplication: selectedApplication,
                SearchTerm: searchTerm,
                TotalCount: filteredUsers.Count()
            );
        }
        catch (ApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }
}
