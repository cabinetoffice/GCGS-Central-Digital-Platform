namespace CO.CDP.OrganisationSync;

using UmPartyRole = CO.CDP.UserManagement.Core.Constants.PartyRole;

/// <summary>Command for claiming an OI person invite — dual-writes OI + UM membership.</summary>
public sealed record ClaimMembershipCommand(
    Guid OrganisationGuid,
    Guid PersonGuid,
    string UserPrincipalId,
    IReadOnlyList<string> InviteScopes,
    IReadOnlyCollection<UmPartyRole> OrganisationPartyRoles);

/// <summary>Command for creating a founder membership when registering a new organisation.</summary>
public sealed record CreateFounderCommand(
    Guid OrganisationGuid,
    string OrganisationName,
    Guid PersonGuid,
    string UserPrincipalId,
    IReadOnlyCollection<UmPartyRole> OrganisationPartyRoles);

/// <summary>Command for updating a UM membership role when a person's OI scopes change.</summary>
public sealed record UpdateMembershipScopesCommand(
    Guid OrganisationGuid,
    Guid PersonGuid,
    IReadOnlyList<string> NewScopes);

/// <summary>Command for removing a UM membership when a person is removed from an OI organisation.</summary>
public sealed record RemoveMembershipCommand(
    Guid OrganisationGuid,
    Guid PersonGuid);
