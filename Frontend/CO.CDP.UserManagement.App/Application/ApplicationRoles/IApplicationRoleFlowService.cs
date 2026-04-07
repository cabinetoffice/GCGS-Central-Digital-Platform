using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Services;

namespace CO.CDP.UserManagement.App.Application.ApplicationRoles
{
    public interface IApplicationRoleFlowService
    {
        Task<ChangeUserApplicationRolesViewModel?> GetUserViewModelAsync(
            string organisationSlug,
            Guid cdpPersonId,
            CancellationToken ct);

        Task<ChangeUserApplicationRolesViewModel?> GetInviteViewModelAsync(
            string organisationSlug,
            Guid inviteGuid,
            CancellationToken ct);

        ChangeApplicationRolesCheckViewModel BuildCheckViewModel(
            ChangeApplicationRoleState state);

        IReadOnlyList<ApplicationRoleAssignmentPostModel> BuildAssignments(
            ChangeApplicationRoleState state);

        Task<Result<ServiceFailure, ServiceOutcome>> UpdateUserRolesAsync(
            string organisationSlug,
            Guid cdpPersonId,
            IReadOnlyList<ApplicationRoleAssignmentPostModel> assignments,
            CancellationToken ct);

        Task<Result<ServiceFailure, ServiceOutcome>> UpdateInviteRolesAsync(
            string organisationSlug,
            Guid inviteGuid,
            IReadOnlyList<ApplicationRoleAssignmentPostModel> assignments,
            CancellationToken ct);
    }
}
