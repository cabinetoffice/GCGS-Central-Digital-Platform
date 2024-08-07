using CO.CDP.AwsServices;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.EntityVerificationClient;
using CO.CDP.Forms.WebApiClient;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp;
using CO.CDP.Person.WebApiClient;
using CO.CDP.Tenant.WebApiClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using static IdentityModel.OidcConstants;
using ISession = CO.CDP.OrganisationApp.ISession;

const string FormsHttpClientName = "FormsHttpClient";
const string TenantHttpClientName = "TenantHttpClient";
const string OrganisationHttpClientName = "OrganisationHttpClient";
const string PersonHttpClientName = "PersonHttpClient";
const string OrganisationAuthorityHttpClientName = "OrganisationAuthorityHttpClient";
const string EvHttpClient = "EvHttpClient";


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages()
    .AddSessionStateTempDataProvider()
    .AddRazorRuntimeCompilation();
builder.ConfigureForwardedHeaders();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(builder.Configuration.GetValue<double>("SessionTimeoutInMinutes"));
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
builder.Services.AddTransient<ApiBearerTokenHandler>();
builder.Services.AddTransient<IFormsEngine, FormsEngine>();

var formsServiceUrl = builder.Configuration.GetValue<string>("FormsService")
            ?? throw new Exception("Missing configuration key: FormsService.");
builder.Services.AddHttpClient(FormsHttpClientName)
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

var evServiceUrl = builder.Configuration.GetValue<string>("EvService")
            ?? throw new Exception("Missing configuration key: EvService.");
builder.Services.AddHttpClient(EvHttpClient)
    .AddHttpMessageHandler<ApiBearerTokenHandler>();
builder.Services.AddTransient<IPponClient, PponClient>(
    sc => new PponClient(evServiceUrl,
        sc.GetRequiredService<IHttpClientFactory>().CreateClient(EvHttpClient)));

var organisationAuthority = builder.Configuration.GetValue<Uri>("Organisation:Authority")
            ?? throw new Exception("Missing configuration key: Organisation:Authority.");
builder.Services.AddHttpClient(OrganisationAuthorityHttpClientName, c => { c.BaseAddress = organisationAuthority; });

builder.Services.AddTransient<OidcEvents>();

var oneLoginAuthority = builder.Configuration.GetValue<string>("OneLogin:Authority")
            ?? throw new Exception("Missing configuration key: OneLogin:Authority.");

var oneLoginClientId = builder.Configuration.GetValue<string>("OneLogin:ClientId")
            ?? throw new Exception("Missing configuration key: OneLogin:ClientId.");
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
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

builder.Services.AddHealthChecks();
builder.Services
    .AddAwsConfiguration(builder.Configuration)
    .AddAwsS3Service();

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

app.MapHealthChecks("/health");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapRazorPages();

app.MapFallback(ctx =>
{
    ctx.Response.Redirect("/page-not-found");
    return Task.CompletedTask;
});

app.Run();