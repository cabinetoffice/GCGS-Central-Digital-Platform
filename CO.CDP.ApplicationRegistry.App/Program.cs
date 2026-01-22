using CO.CDP.ApplicationRegistry.App;
using CO.CDP.ApplicationRegistry.App.Services;
using CO.CDP.Authentication;
using CO.CDP.Authentication.Http;
using CO.CDP.AwsServices;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using static IdentityModel.OidcConstants;

var builder = WebApplication.CreateBuilder(args);

// Routing configuration
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.ConstraintMap["snake_case"] = typeof(SnakeCaseParameterTransformer);
});

builder.Services.AddControllersWithViews(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new SnakeCaseParameterTransformer()));
});

builder.Services.AddRazorPages();
builder.Services.AddGovUkFrontend();

// AWS configuration
builder.Services.AddAwsConfiguration(builder.Configuration);

// Logging, data protection, and session configuration
builder.Services
    .AddLoggingConfiguration(builder.Configuration)
    .AddAmazonCloudWatchLogsService()
    .AddCloudWatchSerilog(builder.Configuration)
    .AddDataProtection()
        .SetApplicationName("CDP-Frontends")
        .PersistKeysToAWSSystemsManager(
            builder.Configuration.GetValue<string>("Aws:SystemManager:DataProtectionPrefix")
            ?? throw new InvalidOperationException("Missing configuration key: Aws:SystemManager:DataProtectionPrefix."))
    .Services
    .AddSharedSessions(builder.Configuration);

builder.Services.AddScoped<IAppSession, Session>();
builder.Services.AddCdpAuthentication(builder.Configuration);

// API client configuration with bearer token handler
var apiBaseUrl = builder.Configuration["ApplicationRegistryApi:BaseUrl"]
    ?? throw new InvalidOperationException("Missing configuration key: ApplicationRegistryApi:BaseUrl.");

builder.Services.AddHttpClient("ApplicationRegistryHttpClient")
    .AddHttpMessageHandler<AuthorityBearerTokenHandler>();

builder.Services.AddHttpClient<CO.CDP.ApplicationRegistry.WebApiClient.ApplicationRegistryClient>()
    .ConfigureHttpClient((sp, client) =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
    })
    .AddHttpMessageHandler<AuthorityBearerTokenHandler>();
builder.Services.AddTransient(sp =>
    new CO.CDP.ApplicationRegistry.WebApiClient.ApplicationRegistryClient(apiBaseUrl, sp.GetRequiredService<System.Net.Http.HttpClient>()));

// Authentication configuration
var oneLoginAuthority = builder.Configuration.GetValue<string>("OneLogin:Authority")
    ?? throw new InvalidOperationException("Missing configuration key: OneLogin:Authority.");
var oneLoginClientId = builder.Configuration.GetValue<string>("OneLogin:ClientId")
    ?? throw new InvalidOperationException("Missing configuration key: OneLogin:ClientId.");
var cookieSecurePolicy = builder.Environment.IsDevelopment()
    ? CookieSecurePolicy.SameAsRequest
    : CookieSecurePolicy.Always;

var sessionTimeoutInMinutes = builder.Configuration.GetValue<double>("SessionTimeoutInMinutes", 60);
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(sessionTimeoutInMinutes);
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.EventsType = typeof(CO.CDP.Authentication.Services.CookieEventsService);
})
.AddOpenIdConnect(options =>
{
    options.Authority = oneLoginAuthority;
    options.ClientId = oneLoginClientId;
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.ResponseMode = OpenIdConnectResponseMode.Query;
    options.Scope.Clear();
    options.Scope.Add(StandardScopes.OpenId);
    options.Scope.Add(StandardScopes.Phone);
    options.Scope.Add(StandardScopes.Email);
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.UsePkce = false;
    options.ClaimActions.MapAll();

    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
    options.CorrelationCookie.SecurePolicy = cookieSecurePolicy;
    options.CorrelationCookie.HttpOnly = true;

    options.NonceCookie.SameSite = SameSiteMode.Lax;
    options.NonceCookie.SecurePolicy = cookieSecurePolicy;
    options.NonceCookie.HttpOnly = true;
});

// Application services
builder.Services.AddScoped<IApplicationService, ApplicationService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.UseGovUkFrontend();

app.MapControllerRoute(
    name: "organisation",
    pattern: "organisation/{organisationId:guid}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

await app.RunAsync();
