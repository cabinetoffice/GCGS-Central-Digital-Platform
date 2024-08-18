using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.Organisation.Authority;
using CO.CDP.Organisation.Authority.AutoMapper;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureForwardedHeaders();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => { o.DocumentApi(builder.Configuration); });
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("OrganisationInformationDatabase") ?? "");
builder.Services.AddProblemDetails();

builder.Services.AddAutoMapper(typeof(WebApiToPersistenceProfile));

builder.Services.AddDbContext<OrganisationInformationContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("OrganisationInformationDatabase") ?? ""));
builder.Services.AddScoped<ITenantRepository, DatabaseTenantRepository>();
builder.Services.AddScoped<IAuthorityRepository, DatabaseAuthorityRepository>();
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();
app.UseForwardedHeaders();
app.MapHealthChecks("/health");
app.UseHttpsRedirection();

if (builder.Configuration.GetValue("Features:SwaggerUI", false))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if(!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseIdentity();

app.Run();
public abstract partial class Program;