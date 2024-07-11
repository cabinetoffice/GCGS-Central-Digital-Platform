using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.Authentication.AuthorizationPolicy;

public class ESenderAuthorizeAttribute : AuthorizeAttribute
{
    public ESenderAuthorizeAttribute()
        : base(Constants.ESenderPolicy)
    {
    }
}