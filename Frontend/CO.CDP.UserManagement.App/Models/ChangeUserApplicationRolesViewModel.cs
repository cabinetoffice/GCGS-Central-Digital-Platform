namespace CO.CDP.UserManagement.App.Models;

public sealed class ChangeUserApplicationRolesViewModel
{
    public string OrganisationSlug { get; init; } = string.Empty;
    public string UserDisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool IsPending { get; init; }
    public Guid? CdpPersonId { get; init; }
    public Guid? InviteGuid { get; init; }
    public List<ApplicationRoleChangeViewModel> Applications { get; init; } = [];
}

public sealed class ApplicationRoleChangeViewModel
{
    public int OrganisationApplicationId { get; init; }
    public string ApplicationName { get; init; } = string.Empty;
    public string ApplicationDescription { get; init; } = string.Empty;
    public int? SelectedRoleId { get; set; }
    public IReadOnlyList<ApplicationRoleOptionViewModel> Roles { get; init; } = [];
}

public sealed class ApplicationRoleChangePostModel
{
    public List<ApplicationRoleAssignmentPostModel> Applications { get; init; } = [];
}

public sealed class ApplicationRoleAssignmentPostModel
{
    public int OrganisationApplicationId { get; init; }
    public int? SelectedRoleId { get; init; }
}
