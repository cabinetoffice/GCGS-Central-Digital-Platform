namespace CO.CDP.OrganisationApp.Pages.Applications;

/// <summary>
/// Session state for the assign-user / edit-roles journey, stored in TempData.
/// Kept across the AssignUser → CheckAnswers redirect pair.
/// </summary>
public class UserAssignmentState
{
    public const string TempDataKey = "UserAssignmentTempData";

    public Guid ApplicationId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>The user's URN / UserPrincipalId from the AppRegistry member list.</summary>
    public string UserPrincipalId { get; set; } = string.Empty;

    /// <summary>Display name resolved from the member list at selection time.</summary>
    public string UserDisplayName { get; set; } = string.Empty;

    public IList<Guid> SelectedRoleIds { get; set; } = [];
    public IList<string> SelectedRoleNames { get; set; } = [];

    /// <summary>True when editing an existing assignment (PUT), false for new (POST).</summary>
    public bool IsEdit { get; set; }
}
