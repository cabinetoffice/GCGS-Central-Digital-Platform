using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.App.Models;

public sealed record JoinRequestConfirmViewModel(
    Guid OrganisationId,
    Guid JoinRequestId,
    Guid PersonId,
    string FullName,
    string Email,
    JoinRequestAction Action);