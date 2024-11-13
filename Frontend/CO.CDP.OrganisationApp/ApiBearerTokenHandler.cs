using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using System.Net.Http.Headers;

namespace CO.CDP.OrganisationApp;

public class ApiBearerTokenHandler(
    ISession session,
    IAuthorityClient authorityClient) : DelegatingHandler
{
    private readonly SemaphoreSlim semaphore = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var userUrn = session.Get<UserDetails>(Session.UserDetailsKey)?.UserUrn;
            var tokens = await authorityClient.GetAuthTokens(userUrn);

            if (tokens != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
            }
        }
        finally
        {
            semaphore.Release();
        }

        return await base.SendAsync(request, cancellationToken);
    }
}