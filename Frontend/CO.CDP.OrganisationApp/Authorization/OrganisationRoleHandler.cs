using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace CO.CDP.OrganisationApp.Authorization;

public class OrganizationRoleHandler : AuthorizationHandler<OrganizationRoleRequirement>
{
    private IOrganisationClient _organisationClient;
    private ISession _session;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrganizationRoleHandler(IOrganisationClient organisationClient, ISession session, IHttpContextAccessor httpContextAccessor)
    {
        _organisationClient = organisationClient;
        _session = session;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OrganizationRoleRequirement requirement)
    {
        var personId = _session.Get<UserDetails>(Session.UserDetailsKey).PersonId;
        try
        {

            var path = _httpContextAccessor.HttpContext.Request.Path.Value;

            if (path != null)
            {
                var pathSegments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (pathSegments.Length >= 2 && pathSegments[0] == "organisation" && Guid.TryParse(pathSegments[1], out Guid organisationId))
                {
                    // TODO: Cache this - otherwise we'll be hitting the db every time we check a user's role, multiple times per page
                    var persons = await _organisationClient.GetOrganisationPersonsAsync(organisationId);
                    var person = persons.FirstOrDefault(p => p.Id == personId); // TODO: We need an api client method for fetching a specific person based on id
                    // TODO: Use tenant lookup instead of the above combo

                    if (person != null && person.Scopes.Contains(requirement.Role))
                    {
                        context.Succeed(requirement);
                        return;
                    }
                }
            }

            context.Fail();
        }
        catch (Exception ex)
        {
            context.Fail();
        }

        // TODO: Stop the app throwing 404 page-not-found when auth fails - need to throw a 403?
    }
}