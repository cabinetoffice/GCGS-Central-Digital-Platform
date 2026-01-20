using CO.CDP.ApplicationRegistry.App;
using CO.CDP.ApplicationRegistry.App.Api;
using CO.CDP.ApplicationRegistry.App.Authentication;
using CO.CDP.ApplicationRegistry.App.Services;
using CO.CDP.ApplicationRegistry.App.WebApiClients;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Refit;
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

// Session configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAppSession, Session>();
builder.Services.AddTransient<ITokenService, TokenService>();
builder.Services.AddTransient<IAuthorityClient, AuthorityClient>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ApiBearerTokenHandler>();

// Authority HTTP client
var organisationAuthority = builder.Configuration.GetValue<Uri>("Organisation:Authority")
    ?? throw new InvalidOperationException("Missing configuration key: Organisation:Authority.");
builder.Services.AddHttpClient(AuthorityClient.OrganisationAuthorityHttpClientName, c => { c.BaseAddress = organisationAuthority; });

// API client configuration with bearer token handler
var apiBaseUrl = builder.Configuration["ApplicationRegistryApi:BaseUrl"]
    ?? throw new InvalidOperationException("Missing configuration key: ApplicationRegistryApi:BaseUrl.");

builder.Services.AddHttpClient("ApplicationRegistryHttpClient")
    .AddHttpMessageHandler<ApiBearerTokenHandler>();

builder.Services
    .AddRefitClient<IApplicationsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<ApiBearerTokenHandler>();

builder.Services
    .AddRefitClient<IOrganisationsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<ApiBearerTokenHandler>();

builder.Services
    .AddRefitClient<IOrganisationApplicationsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<ApiBearerTokenHandler>();

builder.Services
    .AddRefitClient<IUserAssignmentsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<ApiBearerTokenHandler>();

// Authentication configuration
var oneLoginAuthority = builder.Configuration.GetValue<string>("OneLogin:Authority")
    ?? throw new InvalidOperationException("Missing configuration key: OneLogin:Authority.");
var oneLoginClientId = builder.Configuration.GetValue<string>("OneLogin:ClientId")
    ?? throw new InvalidOperationException("Missing configuration key: OneLogin:ClientId.");
var cookieSecurePolicy = builder.Environment.IsDevelopment()
    ? CookieSecurePolicy.SameAsRequest
    : CookieSecurePolicy.Always;

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
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
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

await app.RunAsync();
