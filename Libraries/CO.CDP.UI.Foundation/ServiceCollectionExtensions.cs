using CO.CDP.UI.Foundation.Cookies;
using CO.CDP.UI.Foundation.Services;
using CO.CDP.UI.Foundation.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CO.CDP.UI.Foundation;

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

        // Always add HttpContextAccessor as it's required by most UI services
        _services.AddHttpContextAccessor();
    }

    /// <summary>
    /// Adds cookie preferences services with application-specific cookie name
    /// </summary>
    /// <param name="applicationPrefix">The application name to use as prefix for cookie name</param>
    /// <returns>The builder for method chaining</returns>
    public UiFoundationBuilder AddCookiePreferences(string applicationPrefix)
    {
        var cookieSettings = _configuration.GetSection("CookieSettings").Get<CookieSettings>() ?? new CookieSettings();
        cookieSettings.CookieName = $"{applicationPrefix.ToUpperInvariant()}_COOKIES_PREFERENCES_SET";
        _services.AddSingleton(cookieSettings);
        _services.AddScoped<ICookiePreferencesService, CookiePreferencesService>();
        return this;
    }

    /// <summary>
    /// Adds cookie preferences services with custom configuration
    /// </summary>
    /// <param name="configure">Action to configure cookie settings</param>
    /// <returns>The builder for method chaining</returns>
    public UiFoundationBuilder AddCookiePreferences(Action<CookieSettings> configure)
    {
        var cookieSettings = _configuration.GetSection("CookieSettings").Get<CookieSettings>() ?? new CookieSettings();
        configure(cookieSettings);
        _services.AddSingleton(cookieSettings);
        _services.AddScoped<ICookiePreferencesService, CookiePreferencesService>();
        return this;
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
    /// Adds Session services with default configuration
    /// </summary>
    /// <param name="applicationPrefix">The application name to use as a prefix for the session cookie.</param>
    /// <param name="isDevelopment">Whether the environment is development</param>
    /// <returns>The builder for method chaining</returns>
    public UiFoundationBuilder AddSession(string applicationPrefix, bool isDevelopment)
    {
        if (string.IsNullOrWhiteSpace(applicationPrefix))
        {
            throw new ArgumentException("Application prefix must be provided.", nameof(applicationPrefix));
        }

        var sessionTimeoutInMinutes = _configuration.GetValue<double?>("SessionTimeoutInMinutes") ?? 60;
        var cookieSecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

        _services.AddDistributedMemoryCache();
        _services.AddSession(options =>
        {
            options.Cookie.Name = $"{applicationPrefix}.Session";
            options.IdleTimeout = TimeSpan.FromMinutes(sessionTimeoutInMinutes);
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = cookieSecurePolicy;
        });

        _services.AddScoped<ISessionService, SessionService>();

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