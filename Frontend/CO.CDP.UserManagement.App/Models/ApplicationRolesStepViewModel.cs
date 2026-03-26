using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.App.Models;

public sealed class ApplicationRolesStepViewModel
{
    public string OrganisationSlug { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public OrganisationRole OrganisationRole { get; init; } = OrganisationRole.Member;
    public List<ApplicationAccessSelectionViewModel> Applications { get; init; } = new List<ApplicationAccessSelectionViewModel>();
}

public sealed class ApplicationAccessSelectionViewModel
{
    public int OrganisationApplicationId { get; init; }
    public string ApplicationName { get; init; } = string.Empty;
    public string ApplicationDescription { get; init; } = string.Empty;
    public bool AllowsMultipleRoleAssignments { get; init; }
    public bool IsEnabledByDefault { get; init; }
    public bool GiveAccess { get; set; }
    public int? SelectedRoleId { get; set; }
    public List<int> SelectedRoleIds { get; set; } = new List<int>();
    public IReadOnlyList<ApplicationRoleOptionViewModel> Roles { get; init; } = new List<ApplicationRoleOptionViewModel>();
}

public sealed class ApplicationRoleOptionViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public sealed class ApplicationRolesStepPostModel
{
    public List<ApplicationSelectionPostModel> Applications { get; init; } = new List<ApplicationSelectionPostModel>();
}

public sealed class ApplicationSelectionPostModel
{
    public int OrganisationApplicationId { get; init; }
    public bool GiveAccess { get; init; }
    public int? SelectedRoleId { get; init; }
    public List<int> SelectedRoleIds { get; init; } = new List<int>();
}

public sealed class InviteApplicationAssignment
{
    public int OrganisationApplicationId { get; init; }
    public int ApplicationRoleId { get; init; }
    public IReadOnlyList<int>? ApplicationRoleIds { get; init; }
}

public sealed class InviteCheckAnswersViewModel
{
    public string OrganisationSlug { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public OrganisationRole OrganisationRole { get; init; } = OrganisationRole.Member;
    public IReadOnlyList<InviteCheckAnswersApplicationViewModel> Applications { get; init; } = new List<InviteCheckAnswersApplicationViewModel>();
}

public sealed class InviteCheckAnswersApplicationViewModel
{
    public string ApplicationName { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
}

public sealed class InviteSuccessState
{
    public string OrganisationSlug { get; init; } = string.Empty;
    public string OrganisationName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public OrganisationRole OrganisationRole { get; init; } = OrganisationRole.Member;
    public DateTimeOffset DateAdded { get; init; }
    public IReadOnlyList<InviteSuccessApplicationRoleViewModel> Applications { get; init; } = [];
}

public sealed class InviteSuccessViewModel
{
    public string OrganisationSlug { get; init; } = string.Empty;
    public string OrganisationName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public OrganisationRole OrganisationRole { get; init; } = OrganisationRole.Member;
    public DateTimeOffset DateAdded { get; init; }
    public IReadOnlyList<InviteSuccessApplicationRoleViewModel> Applications { get; init; } = [];
}

public sealed class InviteSuccessApplicationRoleViewModel
{
    public string ApplicationName { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
}
