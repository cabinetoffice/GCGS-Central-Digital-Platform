using System.Reflection;
using CO.CDP.Authentication;
using CO.CDP.Authentication.Authorization;
using CO.CDP.AwsServices;
using CO.CDP.Configuration.Assembly;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.Configuration.Helpers;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Person.WebApi;
using CO.CDP.Person.WebApi.Api;
using CO.CDP.Person.WebApi.AutoMapper;
using CO.CDP.Person.WebApi.Model;
using CO.CDP.Person.WebApi.UseCase;
using CO.CDP.WebApi.Foundation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using Npgsql;
using Person = CO.CDP.Person.WebApi.Model.Person;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureForwardedHeaders();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.DocumentPersonApi(builder.Configuration); });

builder.Services.AddAutoMapper(typeof(WebApiToPersistenceProfile));

var connectionString =
    ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase");
builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).MapEnums().Build());
builder.Services.AddDbContext<OrganisationInformationContext>((sp, options) =>
    options.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));
builder.Services.AddHealthChecks().AddNpgSql(sp => sp.GetRequiredService<NpgsqlDataSource>());

builder.Services.AddScoped<IPersonRepository, DatabasePersonRepository>();
builder.Services.AddScoped<IPersonInviteRepository, DatabasePersonInviteRepository>();
builder.Services.AddScoped<IUseCase<RegisterPerson, Person>, RegisterPersonUseCase>();
builder.Services.AddScoped<IUseCase<Guid, Person?>, GetPersonUseCase>();
builder.Services.AddScoped<IUseCase<LookupPerson, Person?>, LookupPersonUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, UpdatePerson), bool>, UpdatePersonUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, ClaimPersonInvite), bool>, ClaimPersonInviteUseCase>();
builder.Services.AddProblemDetails();

builder.Services.AddFeatureManagement(builder.Configuration.GetSection("Features"));

builder.Services.AddJwtBearerAndApiKeyAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddOrganisationAuthorization();
builder.Services.AddScoped<IAuthorizationHandler, ApiKeyScopeAuthorizationHandler>();


builder.Services
    .AddAwsConfiguration(builder.Configuration)
    .AddLoggingConfiguration(builder.Configuration);

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

if ((Assembly.GetEntryAssembly().IsRunAs("CO.CDP.Person.WebApi")) ||
    (Assembly.GetEntryAssembly().IsRunAs("testhost")))
{
    builder.Services
        .AddAwsSqsService()
        .AddMultiQueueOutboxSqsPublisher<OrganisationInformationContext>(
            builder.Configuration,
            enableBackgroundServices: false,
            notificationChannel: "organisation_information_outbox");
}

var app = builder.Build();
app.UseForwardedHeaders();
app.UseErrorHandler(ErrorCodes.Exception4xxMap);

if (builder.Configuration.GetValue("Features:SwaggerUI", false))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStatusCodePages();

app.MapHealthChecks("/health").AllowAnonymous();
app.UseAuthentication();
app.UseAuthorization();
app.UsePersonEndpoints();
app.UsePersonLookupEndpoints();
app.Run();

public abstract partial class Program;