using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;

namespace CO.CDP.UserManagement.App.Application.InviteUsers
{
    public interface IInviteUserFlowService
    {
        Task<InviteUserViewModel?> GetViewModelAsync(
            string organisationSlug,
            InviteUserViewModel? input,
            CancellationToken ct);

        Task<bool> IsEmailAlreadyInOrganisationAsync(
            string organisationSlug,
            string email,
            CancellationToken ct);

        Task<ApplicationRolesStepViewModel?> GetApplicationRolesStepAsync(
            string organisationSlug,
            InviteUserState state,
            CancellationToken ct);

        Task<InviteCheckAnswersViewModel?> GetCheckAnswersViewModelAsync(
            string organisationSlug,
            InviteUserState state,
            CancellationToken ct);

        Task<Result<ServiceFailure, ServiceOutcome>> InviteAsync(
            string organisationSlug,
            InviteUserState state,
            CancellationToken ct);

        Task<Result<ServiceFailure, ServiceOutcome>> ResendInviteAsync(
            string organisationSlug,
            Guid inviteGuid,
            CancellationToken ct);
    }
}
