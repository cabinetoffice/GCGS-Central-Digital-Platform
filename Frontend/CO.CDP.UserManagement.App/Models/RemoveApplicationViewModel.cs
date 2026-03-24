using System.ComponentModel.DataAnnotations;

namespace CO.CDP.UserManagement.App.Models;

public sealed record RemoveApplicationViewModel(
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
