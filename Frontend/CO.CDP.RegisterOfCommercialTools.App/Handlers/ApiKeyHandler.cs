namespace CO.CDP.RegisterOfCommercialTools.App.Handlers;

public class ApiKeyHandler(IConfiguration configuration) : DelegatingHandler
{
    private const string ApiKeyHeaderName = "cdp-api-key";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var apiKey = configuration.GetValue<string>("CommercialToolsApi:ApiKey");

        if (!string.IsNullOrEmpty(apiKey))
        {
            request.Headers.Add(ApiKeyHeaderName, apiKey);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
