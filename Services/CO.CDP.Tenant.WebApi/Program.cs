using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Tenant.WebApi.Api;
using CO.CDP.Tenant.WebApi.AutoMapper;
using CO.CDP.Tenant.WebApi.Model;
using CO.CDP.Tenant.WebApi.UseCase;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Tenant = CO.CDP.Tenant.WebApi.Model.Tenant;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.DocumentTenantApi());
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks();
builder.Services.AddAutoMapper(typeof(WebApiToPersistenceProfile));

builder.Services.AddDbContext<OrganisationInformationContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("OrganisationInformationDatabase") ?? ""));
builder.Services.AddScoped<ITenantRepository, DatabaseTenantRepository>();
builder.Services.AddScoped<IUseCase<RegisterTenant, Tenant>, RegisterTenantUseCase>();
builder.Services.AddScoped<IUseCase<Guid, Tenant?>, GetTenantUseCase>();
builder.Services.AddScoped<IUseCase<string, Tenant?>, LookupTenantUseCase>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = "https://oidc.integration.account.gov.uk";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuerSigningKey = true
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseTenantEndpoints();
app.UseTenantLookupEndpoints();
app.UseUserManagementEndpoints();
app.Run();

public abstract partial class Program;
