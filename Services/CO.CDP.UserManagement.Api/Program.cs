using CO.CDP.UserManagement.Api.Api;
using CO.CDP.UserManagement.Api.Authorization;
using CO.CDP.ApplicationRegistry.Infrastructure;
using CO.CDP.Logging;
using CO.CDP.Authentication;
using CO.CDP.Authentication.Http;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.Person.WebApiClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSanitisedLogging();
builder.Services.AddSwaggerGen(options => { options.DocumentApplicationRegistryApi(builder.Configuration); });
builder.Services.AddHttpContextAccessor();

// Application Registry Infrastructure and Core Services
var connectionString = builder.Configuration.GetConnectionString("ApplicationRegistryDatabase");
var cdpConnectionString = builder.Configuration.GetConnectionString("CdpDatabase");
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");

builder.Services.AddApplicationRegistryInfrastructure(
    connectionString ?? throw new InvalidOperationException("Database connection string not configured"),
    cdpConnectionString);

builder.Services.AddApplicationRegistryCaching(redisConnectionString);

builder.Services.AddCdpAuthentication(builder.Configuration);

var personServiceUrl = builder.Configuration.GetValue<string>("PersonService");
var organisationServiceUrl = builder.Configuration.GetValue<string>("OrganisationService");
if (!string.IsNullOrWhiteSpace(personServiceUrl))
{
    const string personHttpClientName = "PersonClient";
    builder.Services.AddHttpClient(personHttpClientName)
        .AddHttpMessageHandler<ServiceKeyBearerTokenHandler>();
    builder.Services.AddTransient<IPersonClient, PersonClient>(sc => new PersonClient(personServiceUrl,
        sc.GetRequiredService<IHttpClientFactory>().CreateClient(personHttpClientName)));
}
if (!string.IsNullOrWhiteSpace(organisationServiceUrl))
{
    const string organisationHttpClientName = "OrganisationClient";
    builder.Services.AddHttpClient(organisationHttpClientName);
    builder.Services.AddTransient<IOrganisationClient, OrganisationClient>(sc => new OrganisationClient(organisationServiceUrl,
        sc.GetRequiredService<IHttpClientFactory>().CreateClient(organisationHttpClientName)));
}

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.RequireHttpsMetadata = builder.Environment.IsProduction();
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    // Platform Admin - for platform-wide administrative actions
    options.AddPolicy(PolicyNames.PlatformAdmin, policy =>
        policy.RequireRole("PlatformAdmin", "platform-admin"));

    // Service Account - for service-to-service authentication
    options.AddPolicy(PolicyNames.ServiceAccount, policy =>
        policy.RequireClaim("client_id"));

    // Organisation Member - for users who are members of an organisation
    options.AddPolicy(PolicyNames.OrganisationMember, policy =>
        policy.Requirements.Add(new OrganisationMemberRequirement()));

    // Organisation Admin - for users who are admins/owners of an organisation
    options.AddPolicy(PolicyNames.OrganisationAdmin, policy =>
        policy.Requirements.Add(new OrganisationAdminRequirement()));
});

// Register authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, OrganisationMemberHandler>();
builder.Services.AddScoped<IAuthorizationHandler, OrganisationAdminHandler>();

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString ?? throw new InvalidOperationException("Database connection string not configured"));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
