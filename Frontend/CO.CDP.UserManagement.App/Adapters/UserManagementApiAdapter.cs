using ApiClient = CO.CDP.UserManagement.WebApiClient;
using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Mapping;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;

namespace CO.CDP.UserManagement.App.Adapters;

public sealed class UserManagementApiAdapter : IUserManagementApiAdapter
{
    private readonly ApiClient.UserManagementClient _client;

    public UserManagementApiAdapter(ApiClient.UserManagementClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<OrganisationResponse?> GetOrganisationBySlugAsync(string organisationSlug, CancellationToken ct)
    {
        try { return await _client.BySlugAsync(organisationSlug, ct); }
        catch (ApiClient.ApiException ex) when (ex.StatusCode == 404) { return null; }
    }

    public async Task<ICollection<OrganisationUserResponse>> GetUsersAsync(Guid organisationId, CancellationToken ct)
    {
        try { return (await _client.UsersAll2Async(organisationId, ct)) ?? new List<OrganisationUserResponse>(); }
        catch (ApiClient.ApiException ex) when (ex.StatusCode == 404) { return new List<OrganisationUserResponse>(); }
    }

    public async Task<ICollection<PendingOrganisationInviteResponse>> GetInvitesAsync(Guid organisationId, CancellationToken ct)
    {
        try { return (await _client.InvitesAllAsync(organisationId, ct)) ?? new List<PendingOrganisationInviteResponse>(); }
        catch (ApiClient.ApiException ex) when (ex.StatusCode == 404) { return new List<PendingOrganisationInviteResponse>(); }
    }

    public async Task<ICollection<OrganisationApplicationResponse>> GetApplicationsAsync(int organisationId, CancellationToken ct)
    {
        try { return (await _client.ApplicationsAllAsync(organisationId, ct)) ?? new List<OrganisationApplicationResponse>(); }
        catch (ApiClient.ApiException ex) when (ex.StatusCode == 404) { return new List<OrganisationApplicationResponse>(); }
    }

    public async Task<ICollection<RoleResponse>> GetApplicationRolesAsync(int organisationId, int applicationId, CancellationToken ct)
    {
        try { return (await _client.RolesAll2Async(organisationId, applicationId, null, ct)) ?? new List<RoleResponse>(); }
        catch (ApiClient.ApiException ex) when (ex.StatusCode == 404) { return new List<RoleResponse>(); }
    }

    public async Task<ICollection<OrganisationRoleDefinitionResponse>> GetOrganisationRolesAsync(CancellationToken ct)
    {
        try { return (await _client.OrganisationRolesAsync(ct)) ?? new List<OrganisationRoleDefinitionResponse>(); }
        catch (ApiClient.ApiException ex) when (ex.StatusCode == 404) { return new List<OrganisationRoleDefinitionResponse>(); }
    }

    public async Task<OrganisationUserResponse?> GetUserAsync(Guid organisationId, Guid cdpPersonId, CancellationToken ct)
    {
        try { return await _client.UsersGET2Async(organisationId, cdpPersonId, ct); }
        catch (ApiClient.ApiException ex) when (ex.StatusCode == 404) { return null; }
    }

    public async Task<PendingOrganisationInviteResponse?> GetInviteAsync(Guid organisationId, Guid inviteGuid, CancellationToken ct)
    {
        try
        {
            var invites = await _client.InvitesAllAsync(organisationId, ct);
            return invites?.FirstOrDefault(i => i.CdpPersonInviteGuid == inviteGuid);
        }
        catch (ApiClient.ApiException ex) when (ex.StatusCode == 404) { return null; }
    }

    public async Task<Result<ServiceFailure, ServiceOutcome>> InviteUserAsync(Guid organisationId, InviteUserRequest request, CancellationToken ct)
    {
        try
        {
            // Map shared request to generated client request
            var assignments = request.ApplicationAssignments?.Select(a => new ApiClient.ApplicationAssignment(a.ApplicationRoleIds, a.OrganisationApplicationId)).ToList() ?? new List<ApiClient.ApplicationAssignment>();
            var apiRequest = new ApiClient.InviteUserRequest(assignments, request.Email, request.FirstName, request.LastName, request.OrganisationRole);
            await _client.InvitesPOSTAsync(organisationId, apiRequest, ct);
            return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success);
        }
        catch (ApiClient.ApiException ex)
        {
            return ServiceResultMapper.FromApiException(ex);
        }
    }

    public async Task<Result<ServiceFailure, ServiceOutcome>> ResendInviteAsync(Guid organisationId, Guid inviteGuid, CancellationToken ct)
    {
        try
        {
            var invites = await _client.InvitesAllAsync(organisationId, ct);
            var invite = invites?.FirstOrDefault(i => i.CdpPersonInviteGuid == inviteGuid);
            if (invite == null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

            var sharedRequest = new InviteUserRequest
            {
                FirstName = invite.FirstName ?? string.Empty,
                LastName = invite.LastName ?? string.Empty,
                Email = invite.Email,
                OrganisationRole = invite.OrganisationRole,
                ApplicationAssignments = new List<ApplicationAssignment>()
            };

            var inviteResult = await InviteUserAsync(organisationId, sharedRequest, ct).ConfigureAwait(false);
            if (inviteResult.IsLeft())
            {
                return inviteResult; // propagate failure
            }

            await _client.InvitesDELETEAsync(organisationId, invite.PendingInviteId, ct);
            return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success);
        }
        catch (ApiClient.ApiException ex)
        {
            return ServiceResultMapper.FromApiException(ex);
        }
    }

    public async Task<Result<ServiceFailure, ServiceOutcome>> UpdateUserOrganisationRoleAsync(Guid organisationId, Guid cdpPersonId, ChangeOrganisationRoleRequest request, CancellationToken ct)
    {
        try
        {
            var apiRequest = new ApiClient.ChangeOrganisationRoleRequest(request.OrganisationRole);
            await _client.Role2Async(organisationId, cdpPersonId, apiRequest, ct);
            return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success);
        }
        catch (ApiClient.ApiException ex)
        {
            return ServiceResultMapper.FromApiException(ex);
        }
    }

    public async Task<Result<ServiceFailure, ServiceOutcome>> UpdateInviteOrganisationRoleAsync(Guid organisationId, Guid inviteGuid, ChangeOrganisationRoleRequest request, CancellationToken ct)
    {
        try
        {
            var invites = await _client.InvitesAllAsync(organisationId, ct);
            var invite = invites?.FirstOrDefault(i => i.CdpPersonInviteGuid == inviteGuid);
            if (invite == null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

            var apiRequest = new ApiClient.ChangeOrganisationRoleRequest(request.OrganisationRole);
            await _client.RoleAsync(organisationId, invite.PendingInviteId, apiRequest, ct);
            return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success);
        }
        catch (ApiClient.ApiException ex)
        {
            return ServiceResultMapper.FromApiException(ex);
        }
    }

    public async Task<Result<ServiceFailure, ServiceOutcome>> UpdateUserApplicationRolesAsync(int organisationId, Guid cdpPersonId, UpdateUserAssignmentsRequest request, CancellationToken ct)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        try
        {
            var userPrincipalId = cdpPersonId.ToString();
            // Get existing assignments for this user within the organisation
            var existingAssignments = await _client.AssignmentsAllAsync(organisationId, userPrincipalId, ct).ConfigureAwait(false) ?? new List<UserAssignmentResponse>();

            foreach (var assignment in request.Assignments)
            {
                var roleIds = (assignment.RoleIds).Distinct().ToList();
                if (roleIds.Count == 0) continue;

                var existing = existingAssignments.FirstOrDefault(a => a.OrganisationApplicationId == assignment.OrganisationApplicationId);
                if (existing != null)
                {
                    var updateReq = new UpdateAssignmentRolesRequest { RoleIds = roleIds };
                    await _client.AssignmentsPUTAsync(organisationId, userPrincipalId, existing.Id, updateReq, ct).ConfigureAwait(false);
                }
                else
                {
                    if (!assignment.ApplicationId.HasValue)
                        continue; // cannot create without global application id

                    var createReq = new AssignUserToApplicationRequest
                    {
                        ApplicationId = assignment.ApplicationId.Value,
                        RoleIds = roleIds
                    };

                    await _client.AssignmentsPOSTAsync(organisationId, userPrincipalId, createReq, ct).ConfigureAwait(false);
                }
            }

            return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success);
        }
        catch (ApiClient.ApiException ex)
        {
            return ServiceResultMapper.FromApiException(ex);
        }
    }

    public async Task<Result<ServiceFailure, ServiceOutcome>> UpdateInviteApplicationRolesAsync(Guid cdpOrganisationId, Guid inviteGuid, UpdateUserAssignmentsRequest request, CancellationToken ct)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        try
        {
            var invites = await _client.InvitesAllAsync(cdpOrganisationId, ct).ConfigureAwait(false);
            var invite = invites?.FirstOrDefault(i => i.CdpPersonInviteGuid == inviteGuid);
            if (invite == null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

            // Build a new InviteUserRequest (shared type) with the requested assignments
            var apiAssignments = request.Assignments
                .Select(a => new ApiClient.ApplicationAssignment(a.RoleIds.ToList(), a.OrganisationApplicationId))
                .ToList();

            var apiInvite = new ApiClient.InviteUserRequest(apiAssignments, invite.Email, invite.FirstName, invite.LastName, invite.OrganisationRole);

            await _client.InvitesPOSTAsync(cdpOrganisationId, apiInvite, ct).ConfigureAwait(false);
            await _client.InvitesDELETEAsync(cdpOrganisationId, invite.PendingInviteId, ct).ConfigureAwait(false);

            return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success);
        }
        catch (ApiClient.ApiException ex)
        {
            return ServiceResultMapper.FromApiException(ex);
        }
    }

    public async Task<ICollection<UserAssignmentResponse>> GetUserAssignmentsAsync(int organisationId, Guid cdpPersonId, CancellationToken ct)
    {
        try
        {
            var userPrincipalId = cdpPersonId.ToString();
            return (await _client.AssignmentsAllAsync(organisationId, userPrincipalId, ct).ConfigureAwait(false)) ?? new List<UserAssignmentResponse>();
        }
        catch (ApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return new List<UserAssignmentResponse>();
        }
    }

    public async Task<Result<ServiceFailure, ServiceOutcome>> DeleteUserAssignmentAsync(int organisationId, Guid cdpPersonId, int assignmentId, CancellationToken ct)
    {
        try
        {
            var userPrincipalId = cdpPersonId.ToString();
            await _client.AssignmentsDELETEAsync(organisationId, userPrincipalId, assignmentId, ct).ConfigureAwait(false);
            return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success);
        }
        catch (ApiClient.ApiException ex)
        {
            return ServiceResultMapper.FromApiException(ex);
        }
    }

    public Task<Result<ServiceFailure, ServiceOutcome>> RemoveUserAsync(Guid organisationId, Guid cdpPersonId, CancellationToken ct)
    {
        return Task.FromResult(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));
    }

    public async Task<Result<ServiceFailure, ServiceOutcome>> CancelInviteAsync(Guid organisationId, int pendingInviteId, CancellationToken ct)
    {
        try
        {
            await _client.InvitesDELETEAsync(organisationId, pendingInviteId, ct);
            return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success);
        }
        catch (ApiClient.ApiException ex)
        {
            return ServiceResultMapper.FromApiException(ex);
        }
    }
}