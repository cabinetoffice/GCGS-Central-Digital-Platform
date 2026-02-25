using CO.CDP.Authentication;
using CO.CDP.AwsServices;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.Configuration.Helpers;
using CO.CDP.Authentication.Authorization;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Person.WebApi;
using CO.CDP.Person.WebApi.Api;
using CO.CDP.Person.WebApi.AutoMapper;
using CO.CDP.Person.WebApi.Model;
using CO.CDP.Person.WebApi.UseCase;
using CO.CDP.WebApi.Foundation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureForwardedHeaders();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.DocumentPersonApi(builder.Configuration); });

builder.Services.AddAutoMapper(typeof(WebApiToPersistenceProfile));

var connectionString = ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase");
builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).MapEnums().Build());
builder.Services.AddHealthChecks().AddNpgSql(sp => sp.GetRequiredService<NpgsqlDataSource>());
builder.Services.AddDbContext<OrganisationInformationContext>((sp, o) => o.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));

builder.Services.AddScoped<IPersonRepository, DatabasePersonRepository>();
builder.Services.AddScoped<IPersonInviteRepository, DatabasePersonInviteRepository>();
builder.Services.AddScoped<IUseCase<RegisterPerson, CO.CDP.Person.WebApi.Model.Person>, RegisterPersonUseCase>();
builder.Services.AddScoped<IUseCase<Guid, CO.CDP.Person.WebApi.Model.Person?>, GetPersonUseCase>();
builder.Services.AddScoped<IUseCase<LookupPerson, CO.CDP.Person.WebApi.Model.Person?>, LookupPersonUseCase>();
builder.Services.AddScoped<IUseCase<BulkLookupPerson, IReadOnlyDictionary<Guid, CO.CDP.Person.WebApi.Model.BulkLookupPersonResult>>, BulkLookupPersonUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, UpdatePerson), bool>, UpdatePersonUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, ClaimPersonInvite), bool>, ClaimPersonInviteUseCase>();
builder.Services.AddProblemDetails();

builder.Services.AddJwtBearerAndApiKeyAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddOrganisationAuthorization();
builder.Services.AddScoped<IAuthorizationHandler, ApiKeyScopeAuthorizationHandler>();

var organisationSyncEnabled = builder.Configuration.GetValue("Features:OrganisationSyncEnabled", false);
builder.Services
    .AddAwsConfiguration(builder.Configuration)
    .AddLoggingConfiguration(builder.Configuration)
    .AddAwsSqsService();

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

if (organisationSyncEnabled)
{
    builder.Services.AddOutboxSqsPublisher<OrganisationInformationContext>(
        builder.Configuration,
        enableBackgroundServices: organisationSyncEnabled,
        notificationChannel: "person_information_outbox");
}

var app = builder.Build();
app.UseForwardedHeaders();
app.UseErrorHandler(ErrorCodes.Exception4xxMap);

// Configure the HTTP request pipeline.
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
