using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.Authentication.AuthorizationPolicy;

public class OrganisationApiKeyAuthorizeAttribute : AuthorizeAttribute
{
    public OrganisationApiKeyAuthorizeAttribute()
        : base(Constants.OrganisationApiKeyPolicy)
    {
    }
}