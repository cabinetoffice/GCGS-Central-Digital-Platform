using CO.CDP.UserManagement.App;
using CO.CDP.UserManagement.App.Authentication;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.Authentication;
using CO.CDP.Authentication.Http;
using CO.CDP.AwsServices;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.UI.Foundation;
using CO.CDP.UI.Foundation.Middleware;
using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.UserManagement.App.Application.Users;
using CO.CDP.UserManagement.App.Application.Users.Implementations;
using CO.CDP.UserManagement.App.Application.InviteUsers;
using CO.CDP.UserManagement.App.Application.OrganisationRoles;
using CO.CDP.UserManagement.App.Application.ApplicationRoles;
using CO.CDP.UserManagement.App.Application.Removal;
using CO.CDP.UserManagement.App.Application.InviteUsers.Implementations;
using CO.CDP.UserManagement.App.Application.OrganisationRoles.Implementations;
using CO.CDP.UserManagement.App.Application.ApplicationRoles.Implementations;
using CO.CDP.UserManagement.App.Application.Removal.Implementations;
using GovUk.Frontend.AspNetCore;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using CO.CDP.UserManagement.App.Constants;
using CO.CDP.UserManagement.App.Authorization.Requirements;
using CO.CDP.UserManagement.App.Authorization.Handlers;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureForwardedHeaders();
UserManagementAppConfigurationValidator.Validate(builder.Configuration, builder.Environment);

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
builder.Services.AddHealthChecks();
builder.Services.AddGovUkFrontend();
builder.Services.AddHttpContextAccessor();

// AWS configuration
builder.Services.AddAwsConfiguration(builder.Configuration);

// Logging, data protection, and session configuration
builder.Services
    .AddLoggingConfiguration(builder.Configuration)
    .AddAmazonCloudWatchLogsService()
    .AddCloudWatchSerilog(builder.Configuration)
    .AddSharedSessions(builder.Configuration);

builder.Services.AddDataProtection()
    .SetApplicationName("CDP-Frontends")
    .PersistKeysToAWSSystemsManager(
        builder.Configuration.GetValue<string>("Aws:SystemManager:DataProtectionPrefix")
        ?? throw new InvalidOperationException("Missing configuration key: Aws:SystemManager:DataProtectionPrefix."));

var cookieSettings = new CO.CDP.UI.Foundation.Cookies.CookieSettings();
builder.Configuration.GetSection("CookieSettings").Bind(cookieSettings);
builder.Services.AddSingleton(cookieSettings);

builder.Services.AddUiFoundation(builder.Configuration, ui => {
    ui.AddFtsUrlService();
    ui.AddSirsiUrlService();
    ui.AddCookiePreferenceService();
    ui.AddContentSecurityPolicy();
});
builder.Services.AddScoped<CookieAcceptanceMiddleware>();
builder.Services.AddScoped<ISessionManager, CO.CDP.Authentication.Services.SessionService>();
builder.Services.AddCdpAuthentication(builder.Configuration);
builder.Services.AddSingleton<IOneLoginAuthority, OneLoginAuthority>();
builder.Services.AddOptions<OneLoginOptions>()
    .Bind(builder.Configuration.GetSection("OneLogin"))
    .ValidateDataAnnotations()
    .Validate(options => Uri.TryCreate(options.Authority, UriKind.Absolute, out _),
        "OneLogin:Authority must be a valid absolute URI.")
    .ValidateOnStart();

// API client configuration with bearer token handler
var apiBaseUrl = builder.Configuration["UserManagementApi:BaseUrl"]
    ?? throw new InvalidOperationException("Missing configuration key: UserManagementApi:BaseUrl.");

const string apiHttpClientName = "UserManagementHttpClient";
builder.Services.AddHttpClient(apiHttpClientName)
    .AddHttpMessageHandler<AuthorityBearerTokenHandler>();

builder.Services.AddTransient<CO.CDP.UserManagement.WebApiClient.UserManagementClient>(sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(apiHttpClientName);
    return new CO.CDP.UserManagement.WebApiClient.UserManagementClient(apiBaseUrl, httpClient);
});

// Authentication configuration
var oneLoginOptions = builder.Configuration.GetSection("OneLogin").Get<OneLoginOptions>()
                     ?? new OneLoginOptions();
var cookieSecurePolicy = builder.Environment.IsDevelopment()
    ? CookieSecurePolicy.SameAsRequest
    : CookieSecurePolicy.Always;

var sessionTimeoutInMinutes = builder.Configuration.GetValue<double>("SessionTimeoutInMinutes", 60);
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".AspNetCore.CDP.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(sessionTimeoutInMinutes);
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
});

builder.Services.AddCookiePolicy(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.Secure = cookieSecurePolicy;
});

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = cookieSecurePolicy;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;
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
    options.LogoutPath = "/logout";
    options.EventsType = typeof(CO.CDP.Authentication.Services.CookieEventsService);
})
.AddOpenIdConnect(options =>
{
    options.Authority = oneLoginOptions.Authority;
    options.ClientId = oneLoginOptions.ClientId;
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.ResponseMode = OpenIdConnectResponseMode.Query;
    options.SignedOutCallbackPath = "/signout-callback-oidc";
    options.RemoteSignOutPath = "/one-login/back-channel-sign-out";
    options.Scope.Clear();
    options.Scope.Add(OidcConstants.StandardScopes.OpenId);
    options.Scope.Add(OidcConstants.StandardScopes.Phone);
    options.Scope.Add(OidcConstants.StandardScopes.Email);
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.UsePkce = false;
    options.ClaimActions.MapAll();
    options.EventsType = typeof(CO.CDP.Authentication.Services.OidcEventsService);

    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
    options.CorrelationCookie.SecurePolicy = cookieSecurePolicy;
    options.CorrelationCookie.HttpOnly = true;

    options.NonceCookie.SameSite = SameSiteMode.Lax;
    options.NonceCookie.SecurePolicy = cookieSecurePolicy;
    options.NonceCookie.HttpOnly = true;
});

// Authorization policies and handlers for organisation-level checks
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.OrganisationOwnerOrAdmin, policy => policy.Requirements.Add(new OrganisationOwnerOrAdminRequirement()));
    options.AddPolicy(PolicyNames.OrganisationOwner, policy => policy.Requirements.Add(new OrganisationOwnerRequirement()));
    options.AddPolicy(PolicyNames.OrganisationAdmin, policy => policy.Requirements.Add(new OrganisationAdminRequirement()));
});

builder.Services.AddScoped<IAuthorizationHandler, OrganisationOwnerOrAdminHandler>();
builder.Services.AddScoped<IAuthorizationHandler, OrganisationOwnerHandler>();
builder.Services.AddScoped<IAuthorizationHandler, OrganisationAdminHandler>();
builder.Services.AddScoped<IOrganisationRoleService, OrganisationRoleService>();
builder.Services.AddScoped<IUsersQueryService, UsersQueryService>();
builder.Services.AddScoped<IUserDetailsQueryService, UserDetailsQueryService>();
builder.Services.AddScoped<IInviteUserFlowService, InviteUserFlowService>();
builder.Services.AddScoped<IOrganisationRoleFlowService, OrganisationRoleFlowService>();
builder.Services.AddScoped<IApplicationRoleFlowService, ApplicationRoleFlowService>();
builder.Services.AddScoped<IUserRemovalService, UserRemovalService>();

builder.Services.AddScoped<IInviteUserStateStore, InviteUserSessionStore>();
builder.Services.AddScoped<IChangeRoleStateStore, ChangeRoleSessionStore>();
builder.Services.AddScoped<IChangeApplicationRoleStateStore, ChangeApplicationRoleSessionStore>();
builder.Services.AddScoped<IUserManagementApiAdapter, UserManagementApiAdapter>();

var app = builder.Build();
app.UseForwardedHeaders();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseStatusCodePages(context =>
{
    var response = context.HttpContext.Response;
    var requestPath = context.HttpContext.Request.Path;

    if (response.StatusCode == 404 && !requestPath.StartsWithSegments("/page-not-found"))
    {
        response.Redirect("/page-not-found");
    }
    else if (response.StatusCode >= 500 && !requestPath.StartsWithSegments("/error"))
    {
        response.Redirect("/error");
    }

    return Task.CompletedTask;
});

app.UseMiddleware<CookieAcceptanceMiddleware>();
app.UseCookiePolicy();
app.UseStaticFiles();

app.MapHealthChecks("/health").AllowAnonymous();

app.UseRouting();

app.UseContentSecurityPolicy();

app.UseSession();

if (!app.Environment.IsDevelopment())
{
    app.UseAntiforgery();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseGovUkFrontend();

app.MapControllerRoute(
    name: "organisation_by_guid",
    pattern: "organisation/by-id/{cdpOrganisationId:guid}/{controller=Home}/{action=Index}/{id?}");

var diagnosticPage = builder.Configuration.GetValue<string?>("Features:DiagnosticPage:Path", null);
if (builder.Configuration.GetValue("Features:DiagnosticPage:Enabled", false)
    && !string.IsNullOrWhiteSpace(diagnosticPage))
{
    app.MapControllerRoute(
        name: "diagnostic",
        pattern: diagnosticPage.Trim('/'),
        defaults: new { controller = "Diagnostic", action = "Index" });
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.MapFallback(ctx =>
{
    ctx.Response.Redirect("/page-not-found");
    return Task.CompletedTask;
});

await app.RunAsync();
