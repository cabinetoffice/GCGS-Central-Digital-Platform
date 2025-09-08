using CO.CDP.AwsServices;
using CO.CDP.RegisterOfCommercialTools.App;
using CO.CDP.RegisterOfCommercialTools.App.Middleware;
using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.UI.Foundation;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.DataProtection;
using ISession = CO.CDP.RegisterOfCommercialTools.App.ISession;
using Microsoft.FeatureManagement;
using CO.CDP.RegisterOfCommercialTools.App.Constants;
using CO.CDP.UI.Foundation.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFeatureManagement(builder.Configuration.GetSection("Features"));

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Auth/Login");
    options.Conventions.AllowAnonymousToPage("/Auth/Logout");
    options.Conventions.AllowAnonymousToPage("/page-not-found");
});
builder.Services.AddGovUkFrontend();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks();

builder.Services.AddSingleton<ISession, Session>();

var cookieSettings = new CookieSettings();
builder.Configuration.GetSection("CookieSettings").Bind(cookieSettings);
builder.Services.AddSingleton(cookieSettings);
builder.Services.AddScoped<ICookiePreferencesService, CookiePreferencesService>();

builder.Services.AddUiFoundation(builder.Configuration, uiFoundationBuilder =>
{
    uiFoundationBuilder.AddFtsUrlService();
    uiFoundationBuilder.AddSirsiUrlService();
});

builder.Services.AddScoped<CO.CDP.RegisterOfCommercialTools.App.Handlers.BearerTokenHandler>();

builder.Services.AddHttpClient<CO.CDP.RegisterOfCommercialTools.WebApiClient.ICommercialToolsApiClient, CO.CDP.RegisterOfCommercialTools.WebApiClient.CommercialToolsApiClient>(client =>
{
    var url = builder.Configuration.GetValue<string>("CommercialToolsApi:ServiceUrl")
              ?? throw new Exception("Missing CommercialToolsApi:ServiceUrl configuration.");
    client.BaseAddress = new Uri(url);
})
.AddHttpMessageHandler<CO.CDP.RegisterOfCommercialTools.App.Handlers.BearerTokenHandler>();

builder.Services.AddScoped<ISearchService, SearchService>();

var sessionTimeoutInMinutes = builder.Configuration.GetValue<double>("SessionTimeoutInMinutes", 30);
var cookieSecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.Name = "CommercialTools.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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

var dataProtectionBuilder = builder.Services.AddDataProtection()
    .SetApplicationName("CO.CDP.RegisterOfCommercialTools.App");

var dataProtectionPrefix = builder.Configuration.GetValue<string>("Aws:SystemManager:DataProtectionPrefix");
if (!string.IsNullOrEmpty(dataProtectionPrefix))
{
    dataProtectionBuilder.PersistKeysToAWSSystemsManager(dataProtectionPrefix);
}

builder.Services
    .AddAwsConfiguration(builder.Configuration)
    .AddLoggingConfiguration(builder.Configuration)
    .AddAmazonCloudWatchLogsService()
    .AddCloudWatchSerilog(builder.Configuration)
    .AddSharedSessions(builder.Configuration);

var organisationAuthority = builder.Configuration.GetValue<Uri>("Organisation:Authority");

var oneLoginAuthority = builder.Configuration.GetValue<string>("OneLogin:Authority");
var oneLoginClientId = builder.Configuration.GetValue<string>("OneLogin:ClientId");
var oneLoginCallback = builder.Configuration.GetValue<string>("OneLogin:CallbackPath") ?? "/signin-oidc";


using (var tempServiceProvider = builder.Services.BuildServiceProvider())
{
    var authFeatureManager = tempServiceProvider.GetRequiredService<IFeatureManager>();
    var useOneLogin = await authFeatureManager.IsEnabledAsync(FeatureFlags.UseOneLogin);

    if (useOneLogin)
    {
        if (string.IsNullOrEmpty(oneLoginAuthority) || string.IsNullOrEmpty(oneLoginClientId))
        {
            throw new Exception("OneLogin is enabled but missing required configuration: OneLogin:Authority and OneLogin:ClientId");
        }
        builder.Services.AddOneLoginAuthentication(builder.Configuration, builder.Environment);
    }
    else
    {
        builder.Services.AddAwsCognitoAuthentication(builder.Configuration, builder.Environment);
    }
}

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
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseMiddleware<ContentSecurityPolicyMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

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

app.MapFallback(ctx =>
{
    ctx.Response.Redirect("/page-not-found");
    return Task.CompletedTask;
});

app.Run();
