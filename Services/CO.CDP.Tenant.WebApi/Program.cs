using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Tenant.WebApi.Api;
using CO.CDP.Tenant.WebApi.AutoMapper;
using CO.CDP.Tenant.WebApi.Extensions;
using CO.CDP.Tenant.WebApi.Model;
using CO.CDP.Tenant.WebApi.UseCase;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Tenant = CO.CDP.Tenant.WebApi.Model.Tenant;
using TenantLookup = CO.CDP.Tenant.WebApi.Model.TenantLookup;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.DocumentTenantApi(); });

builder.Services.AddHealthChecks();
builder.Services.AddAutoMapper(typeof(WebApiToPersistenceProfile));

builder.Services.AddDbContext<OrganisationInformationContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("OrganisationInformationDatabase") ?? ""));
builder.Services.AddScoped<ITenantRepository, DatabaseTenantRepository>();

builder.Services.AddScoped<IUseCase<RegisterTenant, Tenant>, RegisterTenantUseCase>();
builder.Services.AddScoped<IUseCase<Guid, Tenant?>, GetTenantUseCase>();
builder.Services.AddScoped<IUseCase<string, TenantLookup?>, LookupTenantUseCase>();
builder.Services.AddTenantProblemDetails();

var authority = builder.Configuration["Organisation:Authority"]
    ?? throw new Exception("Missing configuration key: Organisation:Authority.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = authority;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services
    .AddAuthorizationBuilder()
    .SetFallbackPolicy(
        new AuthorizationPolicyBuilder()
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .Build());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
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