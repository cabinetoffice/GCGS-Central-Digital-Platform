using CO.CDP.UserManagement.Shared.Responses;

namespace CO.CDP.UserManagement.App.Application.Users;

/// <summary>
/// Encapsulates the active filter criteria for the users list.
/// </summary>
public sealed record UsersFilter(
    string? Role,
    string? Application,
    string? Search)
{
    public bool IsActive =>
        !string.IsNullOrEmpty(Role) ||
        !string.IsNullOrEmpty(Application) ||
        !string.IsNullOrEmpty(Search);

    public static UsersFilter None => new(null, null, null);
}

/// <summary>
/// Pure, composable filter predicates for the users list.
/// All functions are stateless — no side effects, no mutation.
/// </summary>
public static class UserFilterPipeline
{
    public static bool MatchesRole(string roleEnumName, string? filter) =>
        string.IsNullOrEmpty(filter) ||
        string.Equals(roleEnumName, filter, StringComparison.OrdinalIgnoreCase);

    public static bool MatchesSearch(string fullName, string email, string? search)
    {
        if (string.IsNullOrEmpty(search)) return true;
        var lower = search.ToLowerInvariant();
        return fullName.ToLowerInvariant().Contains(lower) ||
               email.ToLowerInvariant().Contains(lower);
    }

    public static bool UserMatchesApplication(
        IEnumerable<UserAssignmentResponse>? assignments,
        string? applicationSlug)
    {
        if (string.IsNullOrEmpty(applicationSlug)) return true;
        return (assignments ?? Enumerable.Empty<UserAssignmentResponse>())
            .Any(ar =>
                string.Equals(ar.OrganisationApplicationId.ToString(), applicationSlug,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(ar.Application?.ClientId, applicationSlug,
                    StringComparison.OrdinalIgnoreCase));
    }

    public static bool InviteMatchesApplication(
        IEnumerable<InviteApplicationAssignmentResponse>? assignments,
        string? applicationSlug,
        IReadOnlyCollection<OrganisationApplicationResponse> availableApplications)
    {
        if (string.IsNullOrEmpty(applicationSlug)) return true;
        return (assignments ?? Enumerable.Empty<InviteApplicationAssignmentResponse>())
            .Any(ar =>
            {
                var orgApp = availableApplications.FirstOrDefault(a => a.Id == ar.OrganisationApplicationId);
                return string.Equals(ar.OrganisationApplicationId.ToString(), applicationSlug,
                           StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(orgApp?.Application?.ClientId, applicationSlug,
                           StringComparison.OrdinalIgnoreCase);
            });
    }

    public static IReadOnlyList<OrganisationUserResponse> ApplyTo(
        IEnumerable<OrganisationUserResponse> users,
        UsersFilter filter) =>
        users
            .Where(u =>
                MatchesRole(u.OrganisationRole.ToString(), filter.Role) &&
                MatchesSearch($"{u.FirstName} {u.LastName}", u.Email, filter.Search) &&
                UserMatchesApplication(u.ApplicationAssignments, filter.Application))
            .ToList();

    public static IReadOnlyList<PendingOrganisationInviteResponse> ApplyTo(
        IEnumerable<PendingOrganisationInviteResponse> invites,
        UsersFilter filter,
        IReadOnlyCollection<OrganisationApplicationResponse> availableApplications) =>
        invites
            .Where(i =>
                MatchesRole(i.OrganisationRole.ToString(), filter.Role) &&
                MatchesSearch($"{i.FirstName} {i.LastName}", i.Email, filter.Search) &&
                InviteMatchesApplication(i.ApplicationAssignments, filter.Application, availableApplications))
            .ToList();
}
