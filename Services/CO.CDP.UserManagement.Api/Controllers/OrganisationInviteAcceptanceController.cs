using CO.CDP.Authentication;
using CO.CDP.UserManagement.Api.Authorization;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.Api.Controllers;

/// <summary>
/// Controller for accepting organisation user invites via service key authentication.
/// </summary>
[ApiController]
[Route("api/organisations/{cdpOrganisationId:guid}/invites")]
public class OrganisationInviteAcceptanceController : ControllerBase
{
    private readonly IInviteOrchestrationService _inviteOrchestrationService;
    private readonly bool _internalInviteFlowEnabled;
    private readonly ILogger<OrganisationInviteAcceptanceController> _logger;

    public OrganisationInviteAcceptanceController(
        IInviteOrchestrationService inviteOrchestrationService,
        IConfiguration configuration,
        ILogger<OrganisationInviteAcceptanceController> logger)
    {
        _inviteOrchestrationService = inviteOrchestrationService;
        _internalInviteFlowEnabled = configuration.GetValue("Features:InternalInviteFlowEnabled", false);
        _logger = logger;
    }

    /// <summary>
    /// Accepts a pending invite using service-to-service authentication.
    /// </summary>
    [HttpPost("{pendingInviteId:int}/accept")]
    [Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.AuthenticationScheme, Policy = PolicyNames.ServiceKey)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult> AcceptInvite(
        Guid cdpOrganisationId,
        int pendingInviteId,
        AcceptOrganisationInviteRequest request,
        CancellationToken cancellationToken)
    {
        if (!_internalInviteFlowEnabled)
        {
            _logger.LogWarning("Internal invite flow disabled for pending invite {PendingInviteId}", pendingInviteId);
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new ErrorResponse { Message = "Internal invite flow is disabled." });
        }

        try
        {
            await _inviteOrchestrationService.AcceptInviteAsync(
                cdpOrganisationId,
                pendingInviteId,
                request,
                cancellationToken);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }
}
