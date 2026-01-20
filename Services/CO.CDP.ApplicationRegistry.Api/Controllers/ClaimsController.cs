using CO.CDP.ApplicationRegistry.Api.Models;
using CO.CDP.ApplicationRegistry.Core.Interfaces;
using CO.CDP.ApplicationRegistry.Core.Models.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.ApplicationRegistry.Api.Controllers;

/// <summary>
/// Controller for managing user claims.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClaimsController : ControllerBase
{
    private readonly IClaimsCacheService _claimsCacheService;
    private readonly ILogger<ClaimsController> _logger;

    public ClaimsController(
        IClaimsCacheService claimsCacheService,
        ILogger<ClaimsController> logger)
    {
        _claimsCacheService = claimsCacheService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the complete set of claims for a user.
    /// </summary>
    /// <param name="userId">The user principal identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's claims.</returns>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(UserClaims), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserClaims>> GetUserClaims(string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new ErrorResponse { Message = "User ID is required." });
        }

        var claims = await _claimsCacheService.GetUserClaimsAsync(userId, cancellationToken);
        return Ok(claims);
    }

    /// <summary>
    /// Invalidates the claims cache for a user.
    /// </summary>
    /// <param name="userId">The user principal identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("users/{userId}/cache")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> InvalidateCache(string userId, CancellationToken cancellationToken)
    {
        await _claimsCacheService.InvalidateCacheAsync(userId, cancellationToken);
        return NoContent();
    }
}
