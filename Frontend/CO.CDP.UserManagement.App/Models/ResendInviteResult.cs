using CO.CDP.UserManagement.App.Services;

namespace CO.CDP.UserManagement.App.Models;

public record ResendInviteResult(ServiceOutcome Outcome, string? InviteeName = null);