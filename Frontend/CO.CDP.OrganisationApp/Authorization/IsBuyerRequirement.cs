using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.OrganisationApp.Authorization;

/// <summary>
/// Represents an authorization requirement that enforces the user must be an approved buyer.
/// This requirement is used to trigger a handler that checks, via an external API, whether
/// the current user's organisation has an approved buyer status and has signed the latest MOU.
/// </summary>
public class IsBuyerRequirement : IAuthorizationRequirement;