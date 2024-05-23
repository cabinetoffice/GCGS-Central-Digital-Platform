using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace CO.CDP.OrganisationApp;

public class ApiBearerTokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = httpContextAccessor.HttpContext;
        if (context != null)
        {
            var token = await context.GetTokenAsync("access_token");
            // var expiresAt = DateTimeOffset.Parse(await context.GetTokenAsync("expires_at")).ToLocalTime();

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}