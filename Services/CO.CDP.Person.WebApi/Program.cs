using System.Reflection;
using CO.CDP.Authentication;
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
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureForwardedHeaders();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.DocumentPersonApi(builder.Configuration); });
builder.Services.AddHealthChecks()
        .AddNpgSql(ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase"));
builder.Services.AddAutoMapper(typeof(WebApiToPersistenceProfile));

builder.Services.AddDbContext<OrganisationInformationContext>(o =>
    o.UseNpgsql(ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase")));

builder.Services.AddScoped<IPersonRepository, DatabasePersonRepository>();
builder.Services.AddScoped<IPersonInviteRepository, DatabasePersonInviteRepository>();
builder.Services.AddScoped<IUseCase<RegisterPerson, CO.CDP.Person.WebApi.Model.Person>, RegisterPersonUseCase>();
builder.Services.AddScoped<IUseCase<Guid, CO.CDP.Person.WebApi.Model.Person?>, GetPersonUseCase>();
builder.Services.AddScoped<IUseCase<string, CO.CDP.Person.WebApi.Model.Person?>, LookupPersonUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, ClaimPersonInvite), bool>, ClaimPersonInviteUseCase>();
builder.Services.AddProblemDetails();

builder.Services.AddJwtBearerAndApiKeyAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddOrganisationAuthorization();

if (Assembly.GetEntryAssembly().IsRunAs("CO.CDP.Person.WebApi"))
{
    builder.Services
        .AddAwsConfiguration(builder.Configuration)
        .AddLoggingConfiguration(builder.Configuration)
        .AddAmazonCloudWatchLogsService()
        .AddCloudWatchSerilog(builder.Configuration);
}

var app = builder.Build();
app.UseForwardedHeaders();
app.UseMiddleware<CO.CDP.WebApi.Foundation.ExceptionMiddleware>(ErrorCodes.ExceptionMap);

// Configure the HTTP request pipeline.
if (builder.Configuration.GetValue("Features:SwaggerUI", false))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseExceptionHandler();
}

app.UseStatusCodePages();

app.MapHealthChecks("/health").AllowAnonymous();
app.UseAuthentication();
app.UseAuthorization();
app.UsePersonEndpoints();
app.UsePersonLookupEndpoints();
app.Run();
public abstract partial class Program;