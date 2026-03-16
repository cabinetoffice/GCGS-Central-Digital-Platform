using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.Api.Controllers;

[ApiController]
[Route("api/organisation-roles")]
public class OrganisationRolesController(
    IOrganisationRoleService organisationRoleService,
    ILogger<OrganisationRolesController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrganisationRoleDefinitionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<OrganisationRoleDefinitionResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        try
        {
            var definitions = await organisationRoleService.GetActiveAsync(cancellationToken);

            return Ok(definitions.Select(definition => new OrganisationRoleDefinitionResponse
            {
                Id = (OrganisationRole)definition.Id,
                DisplayName = definition.DisplayName,
                Description = definition.Description
            }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve organisation role definitions.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorResponse { Message = "An error occurred retrieving organisation role definitions." });
        }
    }
}
