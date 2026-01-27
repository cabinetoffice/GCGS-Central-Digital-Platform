using CO.CDP.ApplicationRegistry.Api.Api;
using CO.CDP.ApplicationRegistry.Api.Authorization;
using CO.CDP.ApplicationRegistry.Infrastructure;
using CO.CDP.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSanitisedLogging();
builder.Services.AddSwaggerGen(options => { options.DocumentApplicationRegistryApi(builder.Configuration); });

// Application Registry Infrastructure and Core Services
var connectionString = builder.Configuration.GetConnectionString("ApplicationRegistryDatabase");
var cdpConnectionString = builder.Configuration.GetConnectionString("CdpDatabase");
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");

builder.Services.AddApplicationRegistryInfrastructure(
    connectionString ?? throw new InvalidOperationException("Database connection string not configured"),
    cdpConnectionString);

builder.Services.AddApplicationRegistryCaching(redisConnectionString);

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience = builder.Configuration["Authentication:Audience"];
        options.RequireHttpsMetadata = builder.Environment.IsProduction();
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    // Platform Admin - for platform-wide administrative actions
    options.AddPolicy(PolicyNames.PlatformAdmin, policy =>
        policy.RequireRole("PlatformAdmin", "platform-admin"));

    // Service Account - for service-to-service authentication
    options.AddPolicy(PolicyNames.ServiceAccount, policy =>
        policy.RequireClaim("client_id"));

    // Organisation Member - for users who are members of an organisation
    options.AddPolicy(PolicyNames.OrganisationMember, policy =>
        policy.Requirements.Add(new OrganisationMemberRequirement()));

    // Organisation Admin - for users who are admins/owners of an organisation
    options.AddPolicy(PolicyNames.OrganisationAdmin, policy =>
        policy.Requirements.Add(new OrganisationAdminRequirement()));
});

// Register authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, OrganisationMemberHandler>();
builder.Services.AddScoped<IAuthorizationHandler, OrganisationAdminHandler>();

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString ?? throw new InvalidOperationException("Database connection string not configured"));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
