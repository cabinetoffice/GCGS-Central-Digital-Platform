using Microsoft.AspNetCore.Http;

namespace CO.CDP.Authentication;
public class ClaimService(IHttpContextAccessor httpContextAccessor) : IClaimService
{
    public string? GetUserUrn()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
    }

    public string? GetOrganisationId()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirst("org")?.Value;
    }
}