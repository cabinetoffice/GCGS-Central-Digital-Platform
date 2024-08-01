using CO.CDP.Authentication;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Person.WebApi.Api;
using CO.CDP.Person.WebApi.AutoMapper;
using CO.CDP.Person.WebApi.Extensions;
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
    .AddNpgSql(builder.Configuration.GetConnectionString("OrganisationInformationDatabase") ?? "");
builder.Services.AddAutoMapper(typeof(WebApiToPersistenceProfile));

builder.Services.AddDbContext<OrganisationInformationContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("OrganisationInformationDatabase") ?? ""));
builder.Services.AddScoped<IPersonRepository, DatabasePersonRepository>();
builder.Services.AddScoped<IUseCase<RegisterPerson, CO.CDP.Person.WebApi.Model.Person>, RegisterPersonUseCase>();
builder.Services.AddScoped<IUseCase<Guid, CO.CDP.Person.WebApi.Model.Person?>, GetPersonUseCase>();
builder.Services.AddScoped<IUseCase<string, CO.CDP.Person.WebApi.Model.Person?>, LookupPersonUseCase>();
builder.Services.AddPersonProblemDetails();

builder.Services.AddJwtBearerAndApiKeyAuthentication(builder.Configuration, builder.Environment);
//builder.Services.AddAuthorization();
builder.Services.AddOrganisationAuthorization();

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