using CO.CDP.ApplicationRegistry.Api.Models;
using CO.CDP.ApplicationRegistry.Core.Exceptions;
using CO.CDP.ApplicationRegistry.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.ApplicationRegistry.Api.Controllers;

/// <summary>
/// Controller for managing organisations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganisationsController : ControllerBase
{
    private readonly IOrganisationService _organisationService;
    private readonly ILogger<OrganisationsController> _logger;

    public OrganisationsController(
        IOrganisationService organisationService,
        ILogger<OrganisationsController> logger)
    {
        _organisationService = organisationService;
        _logger = logger;
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
    /// Gets an organisation by slug.
    /// </summary>
    /// <param name="slug">The organisation slug.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The organisation if found.</returns>
    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(OrganisationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganisationResponse>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var organisation = await _organisationService.GetBySlugAsync(slug, cancellationToken);
        if (organisation == null)
        {
            return NotFound(new ErrorResponse { Message = $"Organisation with slug '{slug}' not found." });
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
}
