using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.OrganisationApp.Authorization;

/// <summary>
/// Represents an authorization requirement that enforces the user must be an approved buyer
/// who has signed the latest MoU. This is a separate requirement from PartyRoleRequirement
/// to allow for stackable authorization (e.g., approved buyer vs signed MoU buyer).
/// </summary>
public class BuyerMouRequirement : IAuthorizationRequirement;