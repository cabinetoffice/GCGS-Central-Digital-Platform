using System.Reflection;
using CO.CDP.Authentication;
using CO.CDP.AwsServices;
using CO.CDP.Configuration.Assembly;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.Configuration.Helpers;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.RegisterOfCommercialTools.WebApi;
using CO.CDP.RegisterOfCommercialTools.WebApi.Api;
using CO.CDP.RegisterOfCommercialTools.WebApi.UseCases;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<GetCpvChildrenUseCase>();
builder.Services.AddSingleton<ICpvCodeRepository, InMemoryCpvCodeRepository>();

builder.ConfigureForwardedHeaders();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.DocumentCommercialToolsApi(builder.Configuration));
// builder.Services.AddJwtBearerAndApiKeyAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();

var connectionString =
    ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase");

builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).MapEnums().Build());
builder.Services.AddDbContext<OrganisationInformationContext>((sp, o) =>
    o.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));

if (Assembly.GetEntryAssembly().IsRunAs("CO.CDP.Tenant.WebApi"))
{
    builder.Services
        .AddAwsConfiguration(builder.Configuration)
        .AddLoggingConfiguration(builder.Configuration)
        .AddAmazonCloudWatchLogsService()
        .AddCloudWatchSerilog(builder.Configuration);

    builder.Services.AddHealthChecks().AddNpgSql(sp => sp.GetRequiredService<NpgsqlDataSource>());
}

var app = builder.Build();

if (builder.Configuration.GetValue("Features:SwaggerUI", false))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStatusCodePages();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapCpvEndpoint();

app.Run();

public abstract partial class Program { }
