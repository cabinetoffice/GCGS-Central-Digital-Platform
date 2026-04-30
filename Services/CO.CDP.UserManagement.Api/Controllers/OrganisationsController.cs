using CO.CDP.Organisation.WebApiClient;
using CO.CDP.UserManagement.Api.Models;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UmPartyRole = CO.CDP.UserManagement.Shared.Enums.PartyRole;

namespace CO.CDP.UserManagement.Api.Controllers;

/// <summary>
/// Controller for managing organisations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganisationsController : ControllerBase
{
    private readonly ILogger<OrganisationsController> _logger;
    private readonly IOrganisationClient _organisationClient;
    private readonly IOrganisationService _organisationService;

    public OrganisationsController(
        IOrganisationService organisationService,
        ILogger<OrganisationsController> logger,
        IOrganisationClient organisationClient)
    {
        _organisationService = organisationService;
        _logger = logger;
        _organisationClient = organisationClient;
    }

    /// <summary>
    /// Gets all organisations.
    /// </summary>
    /// <returns>Collection of organisations.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrganisationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrganisationResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var organisations = await _organisationService.GetAllAsync(cancellationToken);
        return Ok(organisations.Select(o => o.ToResponse()));
    }

    /// <summary>
    /// Gets an organisation by ID.
    /// </summary>
    /// <param name="id">The organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The organisation if found.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrganisationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganisationResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var organisation = await _organisationService.GetByIdAsync(id, cancellationToken);
        if (organisation == null)
        {
            return NotFound(new ErrorResponse { Message = $"Organisation with ID {id} not found." });
        }

        return Ok(organisation.ToResponse());
    }

    /// <summary>
    /// Gets an organisation by CDP organisation GUID.
    /// </summary>
    /// <param name="cdpGuid">The CDP organisation GUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The organisation if found.</returns>
    [HttpGet("by-cdp-guid/{cdpGuid:guid}")]
    [ProducesResponseType(typeof(OrganisationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganisationResponse>> GetByCdpGuid(Guid cdpGuid,
        CancellationToken cancellationToken)
    {
        var organisation = await _organisationService.GetByCdpGuidAsync(cdpGuid, cancellationToken);
        if (organisation == null)
        {
            return NotFound(new ErrorResponse { Message = $"Organisation with CDP GUID {cdpGuid} not found." });
        }

        return Ok(organisation.ToResponse());
    }

    /// <summary>
    /// Creates a new organisation.
    /// </summary>
    /// <param name="request">The create organisation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created organisation.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrganisationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrganisationResponse>> Create(
        [FromBody] CreateOrganisationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var organisation = await _organisationService.CreateAsync(
                request.CdpOrganisationGuid,
                request.Name,
                request.IsActive,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = organisation.Id },
                organisation.ToResponse());
        }
        catch (DuplicateEntityException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message, Code = "DUPLICATE_ENTITY" });
        }
    }

    /// <summary>
    /// Updates an existing organisation.
    /// </summary>
    /// <param name="id">The organisation identifier.</param>
    /// <param name="request">The update organisation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated organisation.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(OrganisationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganisationResponse>> Update(
        int id,
        [FromBody] UpdateOrganisationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var organisation = await _organisationService.UpdateAsync(
                id,
                request.Name,
                request.IsActive,
                cancellationToken);

            return Ok(organisation.ToResponse());
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes an organisation.
    /// </summary>
    /// <param name="id">The organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _organisationService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Gets the party roles for an organisation by its CDP organisation GUID.
    /// Proxies to the Organisation WebApi using service key authentication.
    /// </summary>
    /// <param name="cdpOrganisationGuid">The CDP organisation GUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The party roles held by the organisation.</returns>
    [HttpGet("{cdpOrganisationGuid:guid}/party-roles")]
    [ProducesResponseType(typeof(IEnumerable<UmPartyRole>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<UmPartyRole>>> GetPartyRoles(
        Guid cdpOrganisationGuid,
        CancellationToken cancellationToken)
    {
        try
        {
            var organisation = await _organisationClient.GetOrganisationAsync(cdpOrganisationGuid);
            if (organisation == null)
            {
                return NotFound(new ErrorResponse
                    { Message = $"Organisation with CDP GUID {cdpOrganisationGuid} not found." });
            }

            var partyRoles = organisation.Roles
                .Select(r => Enum.TryParse<UmPartyRole>(r.ToString(), out var um) ? (UmPartyRole?)um : null)
                .OfType<UmPartyRole>();

            return Ok(partyRoles);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return NotFound(new ErrorResponse
                { Message = $"Organisation with CDP GUID {cdpOrganisationGuid} not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get party roles for organisation {CdpOrganisationGuid}.",
                cdpOrganisationGuid);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse { Message = "An error occurred retrieving party roles." });
        }
    }
}