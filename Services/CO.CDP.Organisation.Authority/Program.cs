using System.Reflection;
using CO.CDP.AwsServices;
using CO.CDP.Configuration.Assembly;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.Configuration.Helpers;
using CO.CDP.Organisation.Authority;
using CO.CDP.Organisation.Authority.AutoMapper;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureForwardedHeaders();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => { o.DocumentApi(builder.Configuration); });
builder.Services.AddProblemDetails();

builder.Services.AddAutoMapper(typeof(WebApiToPersistenceProfile));

var connectionString = ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase");
builder.Services.AddHealthChecks().AddNpgSql(connectionString);
builder.Services.AddDbContext<OrganisationInformationContext>(o => o.UseNpgsql(connectionString));

builder.Services.AddScoped<IPersonRepository, DatabasePersonRepository>();
builder.Services.AddScoped<ITenantRepository, DatabaseTenantRepository>();
builder.Services.AddScoped<IAuthorityRepository, DatabaseAuthorityRepository>();
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<ITokenService, TokenService>();

if (Assembly.GetEntryAssembly().IsRunAs("CO.CDP.Organisation.Authority"))
{
    builder.Services
        .AddAwsConfiguration(builder.Configuration)
        .AddLoggingConfiguration(builder.Configuration)
        .AddAmazonCloudWatchLogsService()
        .AddCloudWatchSerilog(builder.Configuration);
}

var app = builder.Build();
app.UseForwardedHeaders();
app.MapHealthChecks("/health");

if (builder.Configuration.GetValue("Features:SwaggerUI", false))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseIdentity();

app.Run();
public abstract partial class Program;