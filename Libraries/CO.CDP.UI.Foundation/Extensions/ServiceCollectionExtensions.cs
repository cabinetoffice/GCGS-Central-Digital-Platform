using CO.CDP.UI.Foundation.Cookies;
using CO.CDP.UI.Foundation.Pages;
using CO.CDP.UI.Foundation.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CO.CDP.UI.Foundation.Extensions;

/// <summary>
/// Builder for configuring UI Foundation services
/// </summary>
public class UiFoundationBuilder
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;

    internal UiFoundationBuilder(IServiceCollection services, IConfiguration configuration)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _services.AddHttpContextAccessor();
    }


    /// <summary>
    /// Adds FTS URL service with default configuration
    /// </summary>
    /// <returns>The builder for method chaining</returns>
    public UiFoundationBuilder AddFtsUrlService()
    {
        var options = _configuration.GetSection("FtsService").Get<FtsUrlOptions>()
                      ?? throw new InvalidOperationException("FtsService configuration is missing.");
        _services.AddSingleton(options);
        _services.AddScoped<IFtsUrlService, FtsUrlService>();
        return this;
    }

    /// <summary>
    /// Adds FTS URL service with custom configuration
    /// </summary>
    /// <param name="configure">Action to configure FTS URL options</param>
    /// <returns>The builder for method chaining</returns>
    public UiFoundationBuilder AddFtsUrlService(Action<FtsUrlOptions> configure)
    {
        var options = _configuration.GetSection("FtsService").Get<FtsUrlOptions>()
                      ?? throw new InvalidOperationException("FtsService configuration is missing.");
        configure(options);
        _services.AddSingleton(options);
        _services.AddScoped<IFtsUrlService, FtsUrlService>();
        return this;
    }

    /// <summary>
    /// Adds Sirsi URL service with default configuration
    /// </summary>
    /// <returns>The builder for method chaining</returns>
    public UiFoundationBuilder AddSirsiUrlService()
    {
        var options = _configuration.GetSection("SirsiService").Get<SirsiUrlOptions>()
                      ?? throw new InvalidOperationException("SirsiService configuration is missing.");
        _services.AddSingleton(options);
        _services.AddScoped<ISirsiUrlService, SirsiUrlService>();
        return this;
    }

    /// <summary>
    /// Adds Sirsi URL service with custom configuration
    /// </summary>
    /// <param name="configure">Action to configure Sirsi URL options</param>
    /// <returns>The builder for method chaining</returns>
    public UiFoundationBuilder AddSirsiUrlService(Action<SirsiUrlOptions> configure)
    {
        var options = _configuration.GetSection("SirsiService").Get<SirsiUrlOptions>()
                      ?? throw new InvalidOperationException("SirsiService configuration is missing.");
        configure(options);
        _services.AddSingleton(options);
        _services.AddScoped<ISirsiUrlService, SirsiUrlService>();
        return this;
    }

    /// <summary>
    /// Adds Commercial Tools URL service with default configuration
    /// </summary>
    /// <returns>The builder for method chaining</returns>
    public UiFoundationBuilder AddCommercialToolsUrlService()
    {
        var options = _configuration.GetSection("CommercialToolsApp").Get<CommercialToolsUrlOptions>()
                      ?? throw new InvalidOperationException("CommercialToolsApp configuration is missing.");
        _services.AddSingleton(options);
        _services.AddScoped<ICommercialToolsUrlService, CommercialToolsUrlService>();
        return this;
    }

    /// <summary>
    /// Adds Commercial Tools URL service with custom configuration
    /// </summary>
    /// <param name="configure">Action to configure Commercial Tools URL options</param>
    /// <returns>The builder for method chaining</returns>
    public UiFoundationBuilder AddCommercialToolsUrlService(Action<CommercialToolsUrlOptions> configure)
    {
        var options = _configuration.GetSection("CommercialToolsApp").Get<CommercialToolsUrlOptions>()
                      ?? throw new InvalidOperationException("CommercialToolsApp configuration is missing.");
        configure(options);
        _services.AddSingleton(options);
        _services.AddScoped<ICommercialToolsUrlService, CommercialToolsUrlService>();
        return this;
    }

    /// <summary>
    /// Adds AI Tool URL service with default configuration
    /// </summary>
    /// <returns>The builder for method chaining</returns>
    public UiFoundationBuilder AddAiToolUrlService()
    {
        var options = _configuration.GetSection("AiToolApp").Get<AiToolUrlOptions>()
                      ?? throw new InvalidOperationException("AiToolApp configuration is missing.");
        _services.AddSingleton(options);
        _services.AddScoped<IAiToolUrlService, AiToolUrlService>();
        return this;
    }

    /// <summary>
    /// Adds AI Tool URL service with custom configuration
    /// </summary>
    /// <param name="configure">Action to configure AI Tool URL options</param>
    /// <returns>The builder for method chaining</returns>
    public UiFoundationBuilder AddAiToolUrlService(Action<AiToolUrlOptions> configure)
    {
        var options = _configuration.GetSection("AiToolApp").Get<AiToolUrlOptions>()
                      ?? throw new InvalidOperationException("AiToolApp configuration is missing.");
        configure(options);
        _services.AddSingleton(options);
        _services.AddScoped<IAiToolUrlService, AiToolUrlService>();
        return this;
    }

    /// <summary>
    /// Adds Payments URL service with default configuration
    /// </summary>
    /// <returns>The builder for method chaining</returns>
    public UiFoundationBuilder AddPaymentsUrlService()
    {
        var options = _configuration.GetSection("PaymentsApp").Get<PaymentsUrlOptions>()
                      ?? throw new InvalidOperationException("PaymentsApp configuration is missing.");
        _services.AddSingleton(options);
        _services.AddScoped<IPaymentsUrlService, PaymentsUrlService>();
        return this;
    }

    /// <summary>
    /// Adds Payments URL service with custom configuration
    /// </summary>
    /// <param name="configure">Action to configure Payments URL options</param>
    /// <returns>The builder for method chaining</returns>
    public UiFoundationBuilder AddPaymentsUrlService(Action<PaymentsUrlOptions> configure)
    {
        var options = _configuration.GetSection("PaymentsApp").Get<PaymentsUrlOptions>()
                      ?? throw new InvalidOperationException("PaymentsApp configuration is missing.");
        configure(options);
        _services.AddSingleton(options);
        _services.AddScoped<IPaymentsUrlService, PaymentsUrlService>();
        return this;
    }

    /// <summary>
    /// Adds the unified External Service URL builder
    /// </summary>
    /// <returns>The builder for method chaining</returns>
    public UiFoundationBuilder AddExternalServiceUrlBuilder()
    {
        _services.AddScoped<IExternalServiceUrlBuilder, ExternalServiceUrlBuilder>();
        return this;
    }

    /// <summary>
    /// Adds diagnostic page service
    /// </summary>
    /// <typeparam name="TDiagnosticPage">The diagnostic page implementation type</typeparam>
    /// <returns>The builder for method chaining</returns>
    public UiFoundationBuilder AddDiagnosticPage<TDiagnosticPage>()
        where TDiagnosticPage : class, IDiagnosticPage
    {
        _services.AddTransient<IDiagnosticPage, TDiagnosticPage>();
        return this;
    }

    /// <summary>
    /// Adds UI Foundation CookiePreferences service
    /// </summary>
    /// <returns>The builder for method chaining</returns>
    public UiFoundationBuilder AddCookiePreferenceService()
    {
        _services.AddScoped<ICookiePreferencesService, CookiePreferencesService>();
        return this;
    }

    /// <summary>
    /// Adds the shared AppSessionService for generic session access
    /// </summary>
    public UiFoundationBuilder AddAppSessionService()
    {
        _services.AddScoped<IAppSession, AppSessionService>();
        return this;
    }
}

/// <summary>
/// Extension methods for registering UI Foundation services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds and configures UI Foundation services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The application configuration</param>
    /// <param name="configure">Action to configure the UI Foundation services</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUiFoundation(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<UiFoundationBuilder> configure)
    {
        var builder = new UiFoundationBuilder(services, configuration);
        configure(builder);
        return services;
    }
}