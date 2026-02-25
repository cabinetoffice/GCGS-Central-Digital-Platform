using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.App.Models;

public sealed record InviteUserState(
    string OrganisationSlug,
    string Email,
    string FirstName,
    string LastName,
    OrganisationRole OrganisationRole = OrganisationRole.Member,
    IReadOnlyList<InviteApplicationAssignment>? ApplicationAssignments = null);
