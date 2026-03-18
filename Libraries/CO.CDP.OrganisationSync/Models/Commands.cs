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
