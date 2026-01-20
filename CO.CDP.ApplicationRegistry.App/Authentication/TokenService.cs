using Microsoft.AspNetCore.Authentication;

namespace CO.CDP.ApplicationRegistry.App.Authentication;

public class TokenService(IHttpContextAccessor httpContextAccessor) : ITokenService
{
    public async Task<string?> GetTokenAsync(string tokenName)
    {
        if (httpContextAccessor.HttpContext == null) return null;

        return await httpContextAccessor.HttpContext.GetTokenAsync(tokenName);
    }
}

