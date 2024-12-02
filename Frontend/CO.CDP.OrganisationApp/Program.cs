using CO.CDP.AwsServices;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.DataSharing.WebApiClient;
using CO.CDP.EntityVerificationClient;
using CO.CDP.Forms.WebApiClient;
using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp;
using CO.CDP.OrganisationApp.Authentication;
using CO.CDP.OrganisationApp.Authorization;
using CO.CDP.OrganisationApp.Pages;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using CO.CDP.OrganisationApp.ThirdPartyApiClients.CompaniesHouse;
using CO.CDP.OrganisationApp.ThirdPartyApiClients.CharityCommission;
using CO.CDP.OrganisationApp.WebApiClients;
using CO.CDP.Person.WebApiClient;
using CO.CDP.Tenant.WebApiClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Globalization;
using static IdentityModel.OidcConstants;
using static System.Net.Mime.MediaTypeNames;
using ISession = CO.CDP.OrganisationApp.ISession;

const string FormsHttpClientName = "FormsHttpClient";
const string TenantHttpClientName = "TenantHttpClient";
const string OrganisationHttpClientName = "OrganisationHttpClient";
const string DataSharingHttpClientName = "DataSharingHttpClient";
const string PersonHttpClientName = "PersonHttpClient";
const string EvHttpClient = "EvHttpClient";

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en-GB"), new CultureInfo("cy") };

    options.DefaultRequestCulture = new RequestCulture("en-GB");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CustomQueryStringCultureProvider(),
        new CookieRequestCultureProvider()
    };
});

var mvcBuilder = builder.Services.AddRazorPages()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(StaticTextResource));
    })
    .AddSessionStateTempDataProvider();

if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

builder.ConfigureForwardedHeaders();

builder.Services.AddDistributedMemoryCache();

var sessionTimeoutInMinutes = builder.Configuration.GetValue<double>("SessionTimeoutInMinutes");

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(sessionTimeoutInMinutes);
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ISession, Session>();

builder.Services.AddTransient(provider =>
{
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    var factory = provider.GetRequiredService<ITempDataDictionaryFactory>();
    var context = httpContextAccessor.HttpContext;
    return factory.GetTempData(context);
});
builder.Services.AddScoped<ITempDataService, TempDataService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<ApiBearerTokenHandler>();
builder.Services.AddScoped<CultureDelegatingHandler>();
builder.Services.AddScoped<ICompaniesHouseApi, CompaniesHouseApi>();
builder.Services.AddScoped<ICharityCommissionApi, CharityCommissionApi>();

builder.Services.AddKeyedTransient<IChoiceProviderStrategy, ExclusionAppliesToChoiceProviderStrategy>("ExclusionAppliesToChoiceProviderStrategy");
builder.Services.AddKeyedTransient<IChoiceProviderStrategy, DefaultChoiceProviderStrategy>("DefaultChoiceProviderStrategy");
builder.Services.AddTransient<IChoiceProviderService, ChoiceProviderService>();

builder.Services.AddTransient<IFormsEngine, FormsEngine>();
builder.Services.AddTransient<IDiagnosticPage, DiagnosticPage>();
builder.Services.AddScoped<IUserInfoService, UserInfoService>();
builder.Services.AddScoped<IFtsUrlService, FtsUrlService>();


var formsServiceUrl = builder.Configuration.GetValue<string>("FormsService")
            ?? throw new Exception("Missing configuration key: FormsService.");
builder.Services.AddHttpClient(FormsHttpClientName)
    .AddHttpMessageHandler<CultureDelegatingHandler>()
    .AddHttpMessageHandler<ApiBearerTokenHandler>();
builder.Services.AddTransient<IFormsClient, FormsClient>(
    sc => new FormsClient(formsServiceUrl,
        sc.GetRequiredService<IHttpClientFactory>().CreateClient(FormsHttpClientName)));

var tenantServiceUrl = builder.Configuration.GetValue<string>("TenantService")
            ?? throw new Exception("Missing configuration key: TenantService.");
builder.Services.AddHttpClient(TenantHttpClientName)
    .AddHttpMessageHandler<ApiBearerTokenHandler>();
builder.Services.AddTransient<ITenantClient, TenantClient>(
    sc => new TenantClient(tenantServiceUrl,
        sc.GetRequiredService<IHttpClientFactory>().CreateClient(TenantHttpClientName)));

var personServiceUrl = builder.Configuration.GetValue<string>("PersonService")
            ?? throw new Exception("Missing configuration key: PersonService.");
builder.Services.AddHttpClient(PersonHttpClientName)
    .AddHttpMessageHandler<ApiBearerTokenHandler>();
builder.Services.AddTransient<IPersonClient, PersonClient>(
    sc => new PersonClient(personServiceUrl,
        sc.GetRequiredService<IHttpClientFactory>().CreateClient(PersonHttpClientName)));

var organisationServiceUrl = builder.Configuration.GetValue<string>("OrganisationService")
            ?? throw new Exception("Missing configuration key: OrganisationService.");
builder.Services.AddHttpClient(OrganisationHttpClientName)
    .AddHttpMessageHandler<ApiBearerTokenHandler>();
builder.Services.AddTransient<IOrganisationClient, OrganisationClient>(
    sc => new OrganisationClient(organisationServiceUrl,
        sc.GetRequiredService<IHttpClientFactory>().CreateClient(OrganisationHttpClientName)));

var dataSharingServiceUrl = builder.Configuration.GetValue<string>("DataSharingService")
            ?? throw new Exception("Missing configuration key: DataSharingService.");
builder.Services.AddHttpClient(DataSharingHttpClientName)
    .AddHttpMessageHandler<CultureDelegatingHandler>()
    .AddHttpMessageHandler<ApiBearerTokenHandler>();
builder.Services.AddTransient<IDataSharingClient, DataSharingClient>(
    sc => new DataSharingClient(dataSharingServiceUrl,
        sc.GetRequiredService<IHttpClientFactory>().CreateClient(DataSharingHttpClientName)));

var evServiceUrl = builder.Configuration.GetValue<string>("EntityVerificationService")
            ?? throw new Exception("Missing configuration key: EntityVerificationService.");
builder.Services.AddHttpClient(EvHttpClient)
    .AddHttpMessageHandler<ApiBearerTokenHandler>();
builder.Services.AddTransient<IPponClient, PponClient>(
    sc => new PponClient(evServiceUrl,
        sc.GetRequiredService<IHttpClientFactory>().CreateClient(EvHttpClient)));

var organisationAuthority = builder.Configuration.GetValue<Uri>("Organisation:Authority")
            ?? throw new Exception("Missing configuration key: Organisation:Authority.");
builder.Services.AddHttpClient(AuthorityClient.OrganisationAuthorityHttpClientName, c => { c.BaseAddress = organisationAuthority; });

builder.Services.AddTransient<CookieEvents>();
builder.Services.AddSingleton<IOneLoginAuthority, OneLoginAuthority>();
builder.Services.AddTransient<ILogoutManager, LogoutManager>();
builder.Services.AddTransient<ITokenService, TokenService>();
builder.Services.AddTransient<OidcEvents>();
builder.Services.AddTransient<IAuthorityClient, AuthorityClient>();
builder.Services.AddHttpClient(LogoutManager.LogoutCallbackHttpClientName, c => c.Timeout = TimeSpan.FromSeconds(60));

var oneLoginAuthority = builder.Configuration.GetValue<string>("OneLogin:Authority")
            ?? throw new Exception("Missing configuration key: OneLogin:Authority.");

var oneLoginClientId = builder.Configuration.GetValue<string>("OneLogin:ClientId")
            ?? throw new Exception("Missing configuration key: OneLogin:ClientId.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOneLoginAuthority",
        builder => builder.WithOrigins(oneLoginAuthority)
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionTimeoutInMinutes);
    options.SlidingExpiration = true;
    options.EventsType = typeof(CookieEvents);
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
    options.EventsType = typeof(OidcEvents);
    options.ClaimActions.MapAll();
});

builder.Services.AddSingleton<IAuthorizationPolicyProvider, CustomAuthorizationPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, CustomScopeHandler>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();
builder.Services.AddAuthorization();

builder.Services.AddHealthChecks();
builder.Services
    .AddAwsConfiguration(builder.Configuration)
    .AddAwsS3Service()
    .AddLoggingConfiguration(builder.Configuration)
    .AddAmazonCloudWatchLogsService()
    .AddCloudWatchSerilog(builder.Configuration);

// @see DP-723 for details: https://noticingsystem.atlassian.net/browse/DP-723?focusedCommentId=27796
// builder.Services.AddDataProtection()
//    .PersistKeysToAWSSystemsManager("/OrganisationApp/DataProtection");

var app = builder.Build();
app.UseForwardedHeaders();
app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseRequestLocalization();
app.MapHealthChecks("/health").AllowAnonymous();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseSession();
app.UseMiddleware<AuthenticatedSessionAwareMiddleware>();
app.UseAuthorization();
app.MapRazorPages();

app.MapFallback(ctx =>
{
    ctx.Response.Redirect("/page-not-found");
    return Task.CompletedTask;
});

var diagnosticPage = builder.Configuration.GetValue("Features:DiagnosticPage:Path", (string?)default);
if (builder.Configuration.GetValue("Features:DiagnosticPage:Enabled", false)
    && !string.IsNullOrWhiteSpace(diagnosticPage))
{
    app.MapGet(diagnosticPage, async (IDiagnosticPage dp) => Results.Content(await dp.GetContent(), Text.Html));
}

app.Run();

public abstract partial class Program;