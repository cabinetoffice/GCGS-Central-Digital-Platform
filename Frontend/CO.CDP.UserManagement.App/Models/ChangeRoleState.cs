using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.App.Models;

public sealed record ChangeRoleState(
    Guid OrganisationId,
    Guid? CdpPersonId,
    Guid? InviteGuid,
    string UserDisplayName,
    string Email,
    OrganisationRole CurrentRole,
    OrganisationRole SelectedRole);
