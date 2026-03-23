using System.ComponentModel.DataAnnotations;

namespace CO.CDP.UserManagement.App.Models;

public sealed record RevokeApplicationAccessViewModel(
    string OrganisationSlug = "",
    string UserDisplayName = "",
    string UserEmail = "",
    string ApplicationName = "",
    string ApplicationSlug = "",
    int AssignmentId = 0,
    int OrgId = 0,
    string UserPrincipalId = "",
    string RoleName = "",
    DateTimeOffset? AssignedAt = null,
    string? AssignedByName = null,
    Guid? CdpPersonId = null,
    [Required(ErrorMessage = "Select if you want to revoke access")]
    bool? RevokeConfirmed = null);

public sealed record RevokeApplicationAccessSuccessViewModel(
    string OrganisationSlug = "",
    string UserDisplayName = "",
    string ApplicationName = "");
