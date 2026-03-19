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
    public int ApplicationId { get; init; }
    public string ApplicationClientId { get; init; } = string.Empty;
    public string ApplicationName { get; init; } = string.Empty;
    public string ApplicationDescription { get; init; } = string.Empty;
    public bool AllowsMultipleRoleAssignments { get; init; }
    public bool IsEnabledByDefault { get; init; }
    public bool HasExistingAccess { get; init; }
    public bool GiveAccess { get; set; }
    public int? SelectedRoleId { get; set; }
    public List<int> SelectedRoleIds { get; set; } = [];
    public IReadOnlyList<ApplicationRoleOptionViewModel> Roles { get; init; } = [];
}

public sealed class ApplicationRoleChangePostModel
{
    public List<ApplicationRoleAssignmentPostModel> Applications { get; init; } = [];
}

public sealed class ApplicationRoleAssignmentPostModel
{
    public int OrganisationApplicationId { get; init; }
    public int ApplicationId { get; init; }
    public bool GiveAccess { get; init; }
    public int? SelectedRoleId { get; init; }
    public List<int> SelectedRoleIds { get; init; } = [];
}

public sealed class ChangeApplicationRolesCheckViewModel
{
    public string OrganisationSlug { get; init; } = string.Empty;
    public string UserDisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool IsPending { get; init; }
    public Guid? CdpPersonId { get; init; }
    public Guid? InviteGuid { get; init; }
    public IReadOnlyList<ChangedApplicationRoleViewModel> ChangedApplications { get; init; } = [];
}

public sealed class ChangedApplicationRoleViewModel
{
    public string ApplicationName { get; init; } = string.Empty;
    public string CurrentRoleName { get; init; } = string.Empty;
    public string NewRoleName { get; init; } = string.Empty;
    public bool IsNewAssignment { get; init; }
}

public sealed class ChangeApplicationRolesSuccessViewModel
{
    public string OrganisationSlug { get; init; } = string.Empty;
    public string UserDisplayName { get; init; } = string.Empty;
    public IReadOnlyList<ChangedApplicationRoleViewModel> ChangedApplications { get; init; } = [];
}
