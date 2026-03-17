using CO.CDP.UserManagement.Api.Api;
using CO.CDP.UserManagement.Api.Authorization;
using CO.CDP.UserManagement.Infrastructure;
using CO.CDP.UserManagement.Infrastructure.Data;
using CO.CDP.Logging;
using CO.CDP.Authentication;
using CO.CDP.Authentication.Http;
using CO.CDP.AwsServices;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.UserManagement.Api.Validation;
using CO.CDP.Person.WebApiClient;
using CO.CDP.Configuration.Helpers;
using CO.CDP.OrganisationInformation.Persistence;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.FeatureManagement;
using Npgsql;
using System.Reflection;
using CO.CDP.Configuration.Assembly;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureForwardedHeaders();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateApplicationRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSanitisedLogging();
builder.Services.AddSwaggerGen(options => { options.DocumentUserManagementApi(builder.Configuration); });
builder.Services.AddHttpContextAccessor();

// Application Registry Infrastructure and Core Services
var connectionString = ConnectionStringHelper.GetConnectionString(builder.Configuration, "UserManagementDatabase");
var awsConfiguration = builder.Configuration.GetSection("Aws").Get<AwsConfiguration>();
var redisConnectionString = awsConfiguration?.ElastiCache is not null
    ? $"{awsConfiguration.ElastiCache.Hostname}:{awsConfiguration.ElastiCache.Port}"
    : throw new InvalidOperationException("AWS ElastiCache configuration is required but not found. Ensure Aws:ElastiCache:Hostname and Aws:ElastiCache:Port are configured.");
var organisationInformationConnectionString =
    ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase");

builder.Services.AddUserManagementInfrastructure(connectionString);

builder.Services.AddUserManagementCaching(awsConfiguration);

builder.Services.AddFeatureManagement(builder.Configuration.GetSection("Features"));

builder.Services.AddCdpAuthentication(builder.Configuration);

var swaggerEnabled = builder.Configuration.GetValue("Features:SwaggerUI", builder.Environment.IsDevelopment());

builder.Services.AddLoggingConfiguration(builder.Configuration);

var awsSection = builder.Configuration.GetSection("Aws");
if (awsSection.Exists())
{
    builder.Services.AddAwsConfiguration(builder.Configuration);
}

var awsRegion = builder.Configuration["AWS:Region"]
                ?? Environment.GetEnvironmentVariable("AWS_REGION");
if (awsConfiguration?.CloudWatch is not null && (!string.IsNullOrWhiteSpace(awsConfiguration.ServiceURL) ||
                                         !string.IsNullOrWhiteSpace(awsRegion)))
{
    builder.Services
        .AddAmazonCloudWatchLogsService()
        .AddCloudWatchSerilog(builder.Configuration);
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
builder.Services.AddTransient<CO.CDP.Organisation.WebApiClient.IOrganisationClient, CO.CDP.Organisation.WebApiClient.OrganisationClient>(sc =>
    new CO.CDP.Organisation.WebApiClient.OrganisationClient(
        organisationServiceUrl ?? string.Empty,
        sc.GetRequiredService<IHttpClientFactory>().CreateClient(organisationHttpClientName)));
builder.Services.AddTransient<CO.CDP.UserManagement.Core.Interfaces.IOrganisationApiAdapter, CO.CDP.UserManagement.Api.Services.OrganisationApiAdapter>();

builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(organisationInformationConnectionString).MapEnums().Build());
builder.Services.AddDbContext<OrganisationInformationContext>((sp, options) =>
    options.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));

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
    })
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
    .AddNpgSql(connectionString)
    .AddRedis(redisConnectionString);

var app = builder.Build();
app.UseForwardedHeaders();

// Configure the HTTP request pipeline
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

public abstract partial class Program;
