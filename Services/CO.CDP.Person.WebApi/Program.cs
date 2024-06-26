using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Person.WebApi.Api;
using CO.CDP.Person.WebApi.AutoMapper;
using CO.CDP.Person.WebApi.Model;
using CO.CDP.Person.WebApi.UseCase;
using CO.CDP.Person.WebApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureForwardedHeaders();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.DocumentPersonApi(); });
builder.Services.AddHealthChecks();
builder.Services.AddAutoMapper(typeof(WebApiToPersistenceProfile));

builder.Services.AddDbContext<OrganisationInformationContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("OrganisationInformationDatabase") ?? ""));
builder.Services.AddScoped<IPersonRepository, DatabasePersonRepository>();
builder.Services.AddScoped<IUseCase<RegisterPerson, CO.CDP.Person.WebApi.Model.Person>, RegisterPersonUseCase>();
builder.Services.AddScoped<IUseCase<Guid, CO.CDP.Person.WebApi.Model.Person?>, GetPersonUseCase>();
builder.Services.AddScoped<IUseCase<string, CO.CDP.Person.WebApi.Model.Person?>, LookupPersonUseCase>();
builder.Services.AddPersonProblemDetails();

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
    .AddAuthorizationBuilder();
// builder.Services
//     .AddAuthorizationBuilder()
//     .SetFallbackPolicy(
//         new AuthorizationPolicyBuilder()
//             .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
//             .RequireAuthenticatedUser()
//             .Build());

var app = builder.Build();
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
    app.UseExceptionHandler();
}

app.UseStatusCodePages();

app.MapHealthChecks("/health").AllowAnonymous();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UsePersonEndpoints();
app.UsePersonLookupEndpoints();
app.Run();
public abstract partial class Program;