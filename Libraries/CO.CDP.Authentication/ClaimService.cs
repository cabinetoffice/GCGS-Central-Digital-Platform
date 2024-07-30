using Microsoft.AspNetCore.Http;

namespace CO.CDP.Authentication;
public class ClaimService(IHttpContextAccessor httpContextAccessor) : IClaimService
{
    public string? GetUserUrn()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
    }

    public int? GetOrganisationId()
    {
        if(int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst("org")?.Value, out int result))
        {
            return result;
        }
        return null;
    }
}