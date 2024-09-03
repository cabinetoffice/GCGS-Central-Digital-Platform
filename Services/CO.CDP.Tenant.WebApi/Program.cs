using System.Reflection;
using Amazon;
using Amazon.CloudWatchLogs;
using CO.CDP.Authentication;
using CO.CDP.AwsServices;
using CO.CDP.Configuration.Assembly;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.Configuration.Helpers;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Tenant.WebApi.Api;
using CO.CDP.Tenant.WebApi.AutoMapper;
using CO.CDP.Tenant.WebApi.Extensions;
using CO.CDP.Tenant.WebApi.Model;
using CO.CDP.Tenant.WebApi.UseCase;
using Microsoft.EntityFrameworkCore;
using Tenant = CO.CDP.Tenant.WebApi.Model.Tenant;
using TenantLookup = CO.CDP.OrganisationInformation.TenantLookup;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.AwsCloudWatch;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureForwardedHeaders();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.DocumentTenantApi(builder.Configuration); });
builder.Services.AddHealthChecks();

builder.Services.AddAutoMapper(typeof(WebApiToPersistenceProfile));

builder.Services.AddDbContext<OrganisationInformationContext>(o =>
    o.UseNpgsql(
        ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase")));

builder.Services.AddScoped<ITenantRepository, DatabaseTenantRepository>();

builder.Services.AddScoped<IUseCase<RegisterTenant, Tenant>, RegisterTenantUseCase>();
builder.Services.AddScoped<IUseCase<Guid, Tenant?>, GetTenantUseCase>();
builder.Services.AddScoped<IUseCase<TenantLookup?>, LookupTenantUseCase>();
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddTenantProblemDetails();

builder.Services.AddJwtBearerAndApiKeyAuthentication(builder.Configuration, builder.Environment);
//builder.Services.AddAuthorization();
builder.Services.AddOrganisationAuthorization();
builder.Services.AddHttpContextAccessor();

if (Assembly.GetEntryAssembly().IsRunAs("CO.CDP.Tenant.WebApi"))
{
    builder.Services
        .AddAwsConfiguration(builder.Configuration)
        .AddAwsCloudWatchService();

    builder.Services.AddHealthChecks()
        .AddNpgSql(ConnectionStringHelper.GetConnectionString(builder.Configuration,
            "OrganisationInformationDatabase"));
    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
    );

    using var log = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.AmazonCloudWatch(
            logGroup: builder.Configuration["Aws:LogGroup"],
            logStreamPrefix: builder.Configuration["Aws:LogStream"],
            cloudWatchClient: new AmazonCloudWatchLogsClient(),
            textFormatter: new CompactJsonFormatter())
        .WriteTo.Console()
        .CreateLogger();

    log.Verbose("Writing introduction message...");
    log.Information("Hi there! How are you?");
    log.Verbose("Wrote introduction message!");
}

var app = builder.Build();
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
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

app.UseStatusCodePages();

app.MapHealthChecks("/health").AllowAnonymous();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseTenantEndpoints();
app.UseTenantLookupEndpoints();
app.Run();

public abstract partial class Program;