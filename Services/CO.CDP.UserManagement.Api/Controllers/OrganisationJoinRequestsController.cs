using CO.CDP.UserManagement.Api.Authorization;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.Api.Controllers;

/// <summary>
/// Controller for managing pending join requests to an organisation.
/// </summary>
[ApiController]
[Route("api/organisations/{cdpOrganisationId:guid}/join-requests")]
[Authorize(Policy = PolicyNames.OrganisationAdmin)]
public class OrganisationJoinRequestsController(
    IOrganisationApiAdapter organisationApiAdapter,
    IJoinRequestOrchestrationService joinRequestOrchestrationService,
    ICurrentUserService currentUserService,
    ILogger<OrganisationJoinRequestsController> logger) : ControllerBase
{
    /// <summary>
    /// Gets all pending join requests for an organisation.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<JoinRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<JoinRequestResponse>>> GetJoinRequests(
        Guid cdpOrganisationId,
        CancellationToken cancellationToken)
    {
        var requests = await organisationApiAdapter.GetOrganisationJoinRequestsAsync(
            cdpOrganisationId, cancellationToken);

        var response = requests.Select(r => new JoinRequestResponse
        {
            Id = r.Id,
            PersonId = r.PersonId,
            FirstName = r.FirstName,
            LastName = r.LastName,
            Email = r.Email
        });

        return Ok(response);
    }

    /// <summary>
    /// Approves or rejects a join request.
    /// </summary>
    [HttpPut("{joinRequestId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReviewJoinRequest(
        Guid cdpOrganisationId,
        Guid joinRequestId,
        [FromBody] ReviewJoinRequestRequest request,
        CancellationToken cancellationToken)
    {
        var reviewerPrincipalId = currentUserService.GetUserPrincipalId();
        if (string.IsNullOrEmpty(reviewerPrincipalId))
            return Unauthorized();

        try
        {
            switch (request.Decision)
            {
                case JoinRequestDecision.Accepted:
                    await joinRequestOrchestrationService.ApproveJoinRequestAsync(
                        cdpOrganisationId, joinRequestId, request.RequestingPersonId,
                        reviewerPrincipalId, cancellationToken);
                    break;

                case JoinRequestDecision.Rejected:
                    await joinRequestOrchestrationService.RejectJoinRequestAsync(
                        cdpOrganisationId, joinRequestId, request.RequestingPersonId,
                        reviewerPrincipalId, cancellationToken);
                    break;

                default:
                    return BadRequest(new ErrorResponse { Message = $"Unknown decision: {request.Decision}" });
            }

            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            logger.LogWarning(ex, "Entity not found reviewing join request {JoinRequestId}", joinRequestId);
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }
}