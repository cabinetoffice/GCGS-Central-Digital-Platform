using CO.CDP.ApplicationRegistry.App.Models;
using CO.CDP.ApplicationRegistry.App.WebApiClients;
using System.Net.Http.Headers;

namespace CO.CDP.ApplicationRegistry.App;

public class ApiBearerTokenHandler(
    IAppSession session,
    IAuthorityClient authorityClient) : DelegatingHandler
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var userUrn = session.Get<UserDetails>(Session.UserDetailsKey)?.UserUrn;

            if (userUrn != null)
            {
                var result = await authorityClient.GetAuthTokens(userUrn);

                result.Match(
                    _ => { /* Token error - proceed without authorization header */ },
                    tokens => request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken)
                );
            }
        }
        finally
        {
            _semaphore.Release();
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

