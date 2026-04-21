using System.Reflection;
using CO.CDP.Authentication;
using CO.CDP.Authentication.Http;
using CO.CDP.AwsServices;
using CO.CDP.Configuration.Assembly;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.Configuration.Helpers;
using CO.CDP.Logging;
using CO.CDP.MQ;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Person.WebApiClient;
using CO.CDP.UserManagement.Api;
using CO.CDP.UserManagement.Api.Api;
using CO.CDP.UserManagement.Api.Authorization;
using CO.CDP.UserManagement.Api.Events;
using CO.CDP.UserManagement.Api.Events.Handlers;
using CO.CDP.UserManagement.Api.Validation;
using CO.CDP.UserManagement.CdpInfrastructure;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure;
using CO.CDP.UserManagement.Infrastructure.Events;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureForwardedHeaders();
UserManagementApiConfigurationValidator.Validate(builder.Configuration, builder.Environment);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateApplicationRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSanitisedLogging();
builder.Services.AddSwaggerGen(options => { options.DocumentUserManagementApi(builder.Configuration); });
builder.Services.AddHttpContextAccessor();

// Application Registry Infrastructure and Core Services
var umConnectionString = ConnectionStringHelper.GetConnectionString(builder.Configuration, "UserManagementDatabase");
var oiConnectionString =
    ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase");
var awsConfiguration = builder.Configuration.GetSection("Aws").Get<AwsConfiguration>();

builder.Services.AddUserManagementInfrastructure(umConnectionString);

// OrganisationInformationContext is needed for API key authentication
builder.Services.AddDbContext<OrganisationInformationContext>(options =>
    options.UseNpgsql(oiConnectionString));

builder.Services
    .AddAwsConfiguration(builder.Configuration)
    .AddLoggingConfiguration(builder.Configuration);

builder.Services.AddUserManagementCaching(awsConfiguration);

builder.Services.AddFeatureManagement(builder.Configuration.GetSection("Features"));

builder.Services.AddCdpAuthentication(builder.Configuration);

var swaggerEnabled = builder.Configuration.GetValue("Features:SwaggerUI", builder.Environment.IsDevelopment());

var isRunningAsService = Assembly.GetEntryAssembly().IsRunAs("CO.CDP.UserManagement.Api");

if (isRunningAsService || Assembly.GetEntryAssembly().IsRunAs("testhost"))
{
    builder.Services
        .AddAmazonCloudWatchLogsService()
        .AddCloudWatchSerilog(builder.Configuration)
        .AddAwsSqsService()
        .AddUserManagementOutboxPublisher(builder.Configuration, enableBackgroundServices: isRunningAsService)
        .AddSqsDispatcher(
            EventDeserializer.Deserializer,
            enableBackgroundServices: isRunningAsService,
            services =>
            {
                services.AddScoped<ISubscriber<OrganisationRegistered>, OrganisationRegisteredHandler>();
                services.AddScoped<ISubscriber<OrganisationUpdated>, OrganisationUpdatedHandler>();
                services.AddScoped<ISubscriber<PersonRemovedFromOrganisation>, PersonRemovedHandler>();
                services.AddScoped<ISubscriber<PersonScopesUpdated>, PersonScopesUpdatedHandler>();
                services.AddScoped<ISubscriber<PersonInviteClaimed>, PersonInviteClaimedHandler>();
            },
            (services, dispatcher) =>
            {
                dispatcher.Subscribe<OrganisationRegistered>(services);
                dispatcher.Subscribe<OrganisationUpdated>(services);
                dispatcher.Subscribe<PersonRemovedFromOrganisation>(services);
                dispatcher.Subscribe<PersonScopesUpdated>(services);
                dispatcher.Subscribe<PersonInviteClaimed>(services);
            });
}

var personServiceUrl = builder.Configuration.GetValue<string>("PersonService");
const string personHttpClientName = "PersonClient";
builder.Services.AddHttpClient(personHttpClientName)
    .AddHttpMessageHandler<ServiceKeyBearerTokenHandler>();
builder.Services.AddTransient<IPersonClient, PersonClient>(sc => new PersonClient(personServiceUrl ?? string.Empty,
    sc.GetRequiredService<IHttpClientFactory>().CreateClient(personHttpClientName)));

var organisationServiceUrl = builder.Configuration.GetValue<string>("OrganisationService");
const string organisationHttpClientName = "OrganisationClient";
builder.Services.AddHttpClient(organisationHttpClientName)
    .AddHttpMessageHandler<ServiceKeyBearerTokenHandler>();
builder.Services
    .AddTransient<IOrganisationClient,
        OrganisationClient>(sc =>
        new OrganisationClient(
            organisationServiceUrl ?? string.Empty,
            sc.GetRequiredService<IHttpClientFactory>().CreateClient(organisationHttpClientName)));
builder.Services
    .AddTransient<IOrganisationApiAdapter,
        OrganisationApiAdapter>();
builder.Services
    .AddScoped<IPersonApiAdapter,
        PersonApiAdapter>();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearerAuthentication(builder.Configuration, builder.Environment)
    .AddApiKeyAuthentication();

builder.Services.AddApiKeyAuthenticationServices();

// Authorization
builder.Services.AddAuthorization(options =>
{
    // Platform Admin - for platform-wide administrative actions
    options.AddPolicy(PolicyNames.PlatformAdmin, policy =>
        policy.RequireRole("PlatformAdmin", "platform-admin"));

    // Service Account - for service-to-service authentication
    options.AddPolicy(PolicyNames.ServiceAccount, policy =>
        policy.RequireClaim("client_id"));

    // Service Key - for DB-backed service key authentication
    options.AddPolicy(PolicyNames.ServiceKey, policy =>
        policy.RequireClaim(Constants.ClaimType.Channel, Constants.Channel.ServiceKey));

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
    .AddNpgSql(umConnectionString);

var app = builder.Build();
app.UseForwardedHeaders();

// Configure the HTTP request pipeline
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStatusCodePages();

app.MapHealthChecks("/health").AllowAnonymous();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    using var conn = new NpgsqlConnection(umConnectionString);
    conn.Open();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT 1";
    cmd.ExecuteScalar();
    app.Logger.LogInformation("UserManagement database connectivity check: OK");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "UserManagement database connectivity check FAILED: {Message}", ex.Message);
}

app.Lifetime.ApplicationStopping.Register(() =>
    app.Logger.LogWarning("UserManagement API: ApplicationStopping triggered"));

app.Run();

public abstract partial class Program;