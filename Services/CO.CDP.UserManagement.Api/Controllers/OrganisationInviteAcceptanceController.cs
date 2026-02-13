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

    public OrganisationInviteAcceptanceController(
        IInviteOrchestrationService inviteOrchestrationService)
    {
        _inviteOrchestrationService = inviteOrchestrationService;
    }

    /// <summary>
    /// Accepts a pending invite using service-to-service authentication.
    /// </summary>
    [HttpPost("{inviteId:int}/accept")]
    [Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.AuthenticationScheme, Policy = PolicyNames.ServiceKey)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AcceptInvite(
        Guid cdpOrganisationId,
        int inviteId,
        [FromBody] AcceptOrganisationInviteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _inviteOrchestrationService.AcceptInviteAsync(
                cdpOrganisationId,
                inviteId,
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
