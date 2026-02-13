using CO.CDP.UserManagement.Api.Api;
using CO.CDP.UserManagement.Api.Authorization;
using CO.CDP.UserManagement.Infrastructure;
using CO.CDP.Logging;
using CO.CDP.Authentication;
using CO.CDP.Authentication.Http;
using CO.CDP.AwsServices;
using CO.CDP.UserManagement.Api.Validation;
using CO.CDP.Person.WebApiClient;
using CO.CDP.Configuration.Helpers;
using CO.CDP.OrganisationInformation.Persistence;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateApplicationRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSanitisedLogging();
builder.Services.AddSwaggerGen(options => { options.DocumentUserManagementApi(builder.Configuration); });
builder.Services.AddHttpContextAccessor();

// Application Registry Infrastructure and Core Services
var connectionString = builder.Configuration.GetConnectionString("UserManagementDatabase");
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
var organisationInformationConnectionString =
    ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase");

builder.Services.AddUserManagementInfrastructure(
    connectionString ?? throw new InvalidOperationException("Database connection string not configured"));

builder.Services.AddUserManagementCaching(redisConnectionString);

builder.Services.AddCdpAuthentication(builder.Configuration);

var swaggerEnabled = builder.Configuration.GetValue("Features:SwaggerUI", builder.Environment.IsDevelopment());

builder.Services.AddLoggingConfiguration(builder.Configuration);

var awsSection = builder.Configuration.GetSection("Aws");
if (awsSection.Exists())
{
    builder.Services.AddAwsConfiguration(builder.Configuration);
}

var awsConfig = builder.Configuration.GetSection("Aws").Get<AwsConfiguration>();
var awsRegion = builder.Configuration["AWS:Region"]
                ?? Environment.GetEnvironmentVariable("AWS_REGION");
if (awsConfig?.CloudWatch is not null && (!string.IsNullOrWhiteSpace(awsConfig.ServiceURL) ||
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
    .AddNpgSql(connectionString ?? throw new InvalidOperationException("Database connection string not configured"));

var app = builder.Build();

// Configure the HTTP request pipeline
if (swaggerEnabled)
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

public abstract partial class Program;
