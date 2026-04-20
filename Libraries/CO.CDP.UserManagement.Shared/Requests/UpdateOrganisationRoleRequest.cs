using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.Shared.Requests;

public sealed record UpdateOrganisationRoleRequest(
    OrganisationRole OrganisationRole
);