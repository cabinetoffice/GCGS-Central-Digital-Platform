namespace CO.CDP.UserManagement.App.Models;

public sealed record InviteUserState(
    string OrganisationSlug,
    string Email,
    string FirstName,
    string LastName);
