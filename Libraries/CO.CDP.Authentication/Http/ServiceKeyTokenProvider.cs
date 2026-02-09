using Microsoft.Extensions.Configuration;

namespace CO.CDP.Authentication.Http;

public interface IServiceKeyTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}

public sealed class ServiceKeyTokenProvider(IConfiguration configuration)
    : IServiceKeyTokenProvider
{
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var apiKey = configuration["ServiceKey:ApiKey"]
                     ?? throw new InvalidOperationException("Missing configuration key: ServiceKey:ApiKey.");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("ServiceKey:ApiKey is empty.");
        }

        return await Task.FromResult(apiKey);
    }
}

public sealed class ServiceKeyBearerTokenHandler(IServiceKeyTokenProvider tokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var apiKey = await tokenProvider.GetAccessTokenAsync(cancellationToken);
        request.Headers.Remove(ApiKeyAuthenticationHandler.ApiKeyHeaderName);
        request.Headers.Add(ApiKeyAuthenticationHandler.ApiKeyHeaderName, apiKey);
        return await base.SendAsync(request, cancellationToken);
    }
}
