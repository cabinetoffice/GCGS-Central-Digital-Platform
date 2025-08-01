using Microsoft.AspNetCore.Authentication;

namespace CO.CDP.RegisterOfCommercialTools.App.Handlers;

public class BearerTokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            return await base.SendAsync(request, cancellationToken);
        var accessToken = await httpContext.GetTokenAsync("access_token");

        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}