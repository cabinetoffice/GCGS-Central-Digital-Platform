using CO.CDP.Organisation.WebApi.Api;
using CO.CDP.Organisation.WebApi.AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Organisation = CO.CDP.Organisation.WebApi.Model.Organisation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.DocumentOrganisationApi(); });
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks();
builder.Services.AddAutoMapper(typeof(WebApiToPersistenceProfile));

builder.Services.AddDbContext<OrganisationInformationContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("OrganisationInformationDatabase") ?? ""));
builder.Services.AddScoped<IOrganisationRepository, DatabaseOrganisationRepository>();
builder.Services.AddScoped<IPersonRepository, DatabasePersonRepository>();
builder.Services.AddScoped<IUseCase<RegisterOrganisation, Organisation>, RegisterOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<Guid, Organisation?>, GetOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<string, Organisation?>, LookupOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<string, IEnumerable<Organisation>>, GetOrganisationsUseCase>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = builder.Configuration.GetValue<string>("OneLogin:Authority");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuerSigningKey = true
        };
    });
builder.Services.AddAuthorization();

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
app.UseOrganisationEndpoints();
app.UseOrganisationLookupEndpoints();
app.Run();
public abstract partial class Program;