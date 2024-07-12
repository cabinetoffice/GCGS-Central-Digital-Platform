using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.Authentication.AuthorizationPolicy;

public class OrganisationKeyAuthorizeAttribute : AuthorizeAttribute
{
    public OrganisationKeyAuthorizeAttribute()
        : base(Constants.OrganisationKeyPolicy)
    {
    }
}