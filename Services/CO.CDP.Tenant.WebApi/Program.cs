using CO.CDP.Authentication;
using CO.CDP.AwsServices;
using CO.CDP.Configuration.Assembly;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.Configuration.Helpers;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Tenant.WebApi;
using CO.CDP.Tenant.WebApi.Api;
using CO.CDP.Tenant.WebApi.AutoMapper;
using CO.CDP.Tenant.WebApi.Model;
using CO.CDP.Tenant.WebApi.UseCase;
using CO.CDP.WebApi.Foundation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Reflection;
using Tenant = CO.CDP.Tenant.WebApi.Model.Tenant;
using TenantLookup = CO.CDP.OrganisationInformation.TenantLookup;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureForwardedHeaders();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.DocumentTenantApi(builder.Configuration); });
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(WebApiToPersistenceProfile));

var connectionString = ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase");
builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).MapEnums().Build());
builder.Services.AddDbContext<OrganisationInformationContext>((sp, options) =>
{
    var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
    options.UseNpgsql(dataSource);
});

builder.Services.AddScoped<ITenantRepository, DatabaseTenantRepository>();
builder.Services.AddScoped<IPersonRepository, DatabasePersonRepository>();

builder.Services.AddScoped<IUseCase<RegisterTenant, Tenant>, RegisterTenantUseCase>();
builder.Services.AddScoped<IUseCase<Guid, Tenant?>, GetTenantUseCase>();
builder.Services.AddScoped<IUseCase<TenantLookup?>, LookupTenantUseCase>();
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddProblemDetails();

builder.Services.AddJwtBearerAndApiKeyAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddOrganisationAuthorization();

if (Assembly.GetEntryAssembly().IsRunAs("CO.CDP.Tenant.WebApi"))
{
    builder.Services
        .AddAwsConfiguration(builder.Configuration)
        .AddLoggingConfiguration(builder.Configuration)
        .AddAmazonCloudWatchLogsService()
        .AddCloudWatchSerilog(builder.Configuration);

    builder.Services.AddHealthChecks().AddNpgSql(connectionString);
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
app.UseTenantEndpoints();
app.UseTenantLookupEndpoints();
app.Run();

public abstract partial class Program;