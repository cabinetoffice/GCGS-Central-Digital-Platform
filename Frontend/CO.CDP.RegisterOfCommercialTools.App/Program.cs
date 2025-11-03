using CO.CDP.AwsServices;
using CO.CDP.RegisterOfCommercialTools.App;
using CO.CDP.RegisterOfCommercialTools.App.Middleware;
using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.UI.Foundation;
using GovUk.Frontend.AspNetCore;
using ISession = CO.CDP.RegisterOfCommercialTools.App.ISession;
using Microsoft.FeatureManagement;
using CO.CDP.RegisterOfCommercialTools.App.Constants;
using CO.CDP.UI.Foundation.Cookies;
using Microsoft.AspNetCore.DataProtection;
using CO.CDP.Configuration.ForwardedHeaders;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureForwardedHeaders();

builder.Services.AddFeatureManagement(builder.Configuration.GetSection("Features"));

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Auth/Login");
    options.Conventions.AllowAnonymousToPage("/Auth/Logout");
    options.Conventions.AllowAnonymousToPage("/page-not-found");
    options.Conventions.AllowAnonymousToPage("/cookies");
});
builder.Services.AddGovUkFrontend();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks();

builder.Services.AddSingleton<ISession, Session>();

var cookieSettings = new CookieSettings();
builder.Configuration.GetSection("CookieSettings").Bind(cookieSettings);
builder.Services.AddSingleton(cookieSettings);

builder.Services.AddUiFoundation(builder.Configuration, uiFoundationBuilder =>
{
    uiFoundationBuilder.AddFtsUrlService();
    uiFoundationBuilder.AddSirsiUrlService();
    uiFoundationBuilder.AddDiagnosticPage<CO.CDP.RegisterOfCommercialTools.App.Pages.DiagnosticPage>();
    uiFoundationBuilder.AddCookiePreferenceService();
});

builder.Services.AddScoped<CO.CDP.UI.Foundation.Middleware.CookieAcceptanceMiddleware>();

builder.Services.AddScoped<CO.CDP.RegisterOfCommercialTools.App.Handlers.BearerTokenHandler>();
builder.Services.AddScoped<CO.CDP.RegisterOfCommercialTools.App.Handlers.ApiKeyHandler>();

builder.Services.AddHttpClient<CO.CDP.RegisterOfCommercialTools.WebApiClient.ICommercialToolsApiClient, CO.CDP.RegisterOfCommercialTools.WebApiClient.CommercialToolsApiClient>(client =>
{
    var url = builder.Configuration.GetValue<string>("CommercialToolsApi:ServiceUrl")
              ?? throw new Exception("Missing CommercialToolsApi:ServiceUrl configuration.");
    client.BaseAddress = new Uri(url);
})
.AddHttpMessageHandler<CO.CDP.RegisterOfCommercialTools.App.Handlers.ApiKeyHandler>()
.AddHttpMessageHandler<CO.CDP.RegisterOfCommercialTools.App.Handlers.BearerTokenHandler>();

builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ICpvCodeService, CpvCodeService>();
builder.Services.AddScoped<IHierarchicalCodeService<CpvCodeDto>, CpvCodeService>();
builder.Services.AddScoped<ILocationCodeService, LocationCodeService>();
builder.Services.AddScoped<IHierarchicalCodeService<NutsCodeDto>, LocationCodeService>();

builder.Services.AddControllers();

var sessionTimeoutInMinutes = builder.Configuration.GetValue<double>("SessionTimeoutInMinutes", 60);
var cookieSecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(sessionTimeoutInMinutes);
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
});

builder.Services.AddCookiePolicy(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    options.Secure = cookieSecurePolicy;
});

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = cookieSecurePolicy;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;
});

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365); // see https://aka.ms/aspnetcore-hsts
});

builder.Services.AddDataProtection()
   .SetApplicationName("CDP-Frontends")
   .PersistKeysToAWSSystemsManager(
       builder.Configuration.GetValue<string>("Aws:SystemManager:DataProtectionPrefix")
       ?? throw new Exception("Missing configuration key: Aws:SystemManager:DataProtectionPrefix."));

builder.Services
    .AddAwsConfiguration(builder.Configuration)
    .AddLoggingConfiguration(builder.Configuration)
    .AddAmazonCloudWatchLogsService()
    .AddCloudWatchSerilog(builder.Configuration)
    .AddSharedSessions(builder.Configuration);

var oneLoginAuthority = builder.Configuration.GetValue<string>("OneLogin:Authority");
var oneLoginClientId = builder.Configuration.GetValue<string>("OneLogin:ClientId");

if (string.IsNullOrEmpty(oneLoginAuthority) || string.IsNullOrEmpty(oneLoginClientId))
{
    throw new Exception("Missing required OneLogin configuration: OneLogin:Authority and OneLogin:ClientId");
}

builder.Services.AddTransient<CO.CDP.RegisterOfCommercialTools.App.Authentication.OidcEvents>();
builder.Services.AddOneLoginAuthentication(builder.Configuration, builder.Environment);

builder.Services.AddAuthorization();

builder.Configuration.GetValue<string>("CommercialToolsApi:ServiceUrl");
builder.Services.AddScoped<CO.CDP.UI.Foundation.Pages.NotFoundPage>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var featureManager = scope.ServiceProvider.GetRequiredService<IFeatureManager>();
    if (!await featureManager.IsEnabledAsync(FeatureFlags.CommercialTools))
    {
        app.UseStatusCodePagesWithRedirects("/page-not-found");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseMiddleware<ContentSecurityPolicyMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<CO.CDP.UI.Foundation.Middleware.CookieAcceptanceMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseAntiforgery();
}

app.UseCookiePolicy();
app.UseStaticFiles();

app.UseRouting();

app.UseGovUkFrontend();

app.UseAuthentication();
app.UseSession();

app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapRazorPages();

var diagnosticPage = builder.Configuration.GetValue<string?>("Features:DiagnosticPage:Path", null);
if (builder.Configuration.GetValue("Features:DiagnosticPage:Enabled", false)
    && !string.IsNullOrWhiteSpace(diagnosticPage))
{
    app.MapGet(diagnosticPage, async (CO.CDP.UI.Foundation.Pages.IDiagnosticPage dp) => Results.Content(await dp.GetContent(), "text/html"));
}

app.MapControllers();

app.MapFallback(ctx =>
{
    ctx.Response.Redirect("/page-not-found");
    return Task.CompletedTask;
});

app.Run();
