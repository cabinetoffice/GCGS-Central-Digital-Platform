namespace CO.CDP.UI.Foundation.Services;

/// <summary>
/// Service for building URLs to external services using an enum-based approach
/// </summary>
public class ExternalServiceUrlBuilder : IExternalServiceUrlBuilder
{
    private readonly ICommercialToolsUrlService _commercialToolsUrlService;
    private readonly IAiToolUrlService _aiToolUrlService;
    private readonly IPaymentsUrlService _paymentsUrlService;

    /// <summary>
    /// Initialises a new instance of the ExternalServiceUrlBuilder
    /// </summary>
    /// <param name="commercialToolsUrlService">Commercial Tools URL service</param>
    /// <param name="aiToolUrlService">AI Tool URL service</param>
    /// <param name="paymentsUrlService">Payments URL service</param>
    public ExternalServiceUrlBuilder(
        ICommercialToolsUrlService commercialToolsUrlService,
        IAiToolUrlService aiToolUrlService,
        IPaymentsUrlService paymentsUrlService)
    {
        _commercialToolsUrlService = commercialToolsUrlService;
        _aiToolUrlService = aiToolUrlService;
        _paymentsUrlService = paymentsUrlService;
    }

    /// <summary>
    /// Builds a URL to an external service endpoint with optional additional query parameters
    /// </summary>
    /// <param name="service">The external service to build a URL for</param>
    /// <param name="endpoint">The endpoint path</param>
    /// <param name="organisationId">Optional organisation ID</param>
    /// <param name="redirectUri">Optional redirect URI</param>
    /// <param name="cookieAcceptance">Optional cookie acceptance status</param>
    /// <param name="additionalParams">Additional query parameters to include</param>
    /// <returns>The complete URL to the external service endpoint</returns>
    public string BuildUrl(ExternalService service, string endpoint, Guid? organisationId = null,
                          string? redirectUri = null, bool? cookieAcceptance = null,
                          Dictionary<string, string?>? additionalParams = null)
    {
        return service switch
        {
            ExternalService.CommercialTools => _commercialToolsUrlService.BuildUrl(endpoint, organisationId, redirectUri, cookieAcceptance, additionalParams),
            ExternalService.AiTool => _aiToolUrlService.BuildUrl(endpoint, organisationId, redirectUri, cookieAcceptance, additionalParams),
            ExternalService.Payments => _paymentsUrlService.BuildUrl(endpoint, organisationId, redirectUri, cookieAcceptance, additionalParams),
            _ => throw new ArgumentOutOfRangeException(nameof(service), service, $@"Unsupported external service: {service}")
        };
    }
}
