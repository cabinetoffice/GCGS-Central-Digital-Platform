using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Person.WebApi.Api;
using CO.CDP.Person.WebApi.AutoMapper;
using CO.CDP.Person.WebApi.Model;
using CO.CDP.Person.WebApi.UseCase;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.DocumentPersonApi(); });
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks();
builder.Services.AddAutoMapper(typeof(WebApiToPersistenceProfile));

builder.Services.AddScoped<OrganisationInformationContext>(_ => new OrganisationInformationContext(builder.Configuration.GetConnectionString("OrganisationInformationDatabase") ?? ""));
builder.Services.AddScoped<IPersonRepository, DatabasePersonRepository>();
builder.Services.AddScoped<IUseCase<RegisterPerson, CO.CDP.Person.WebApi.Model.Person>, RegisterPersonUseCase>();
builder.Services.AddScoped<IUseCase<Guid, CO.CDP.Person.WebApi.Model.Person?>, GetPersonUseCase>();
builder.Services.AddScoped<IUseCase<string, CO.CDP.Person.WebApi.Model.Person?>, LookupPersonUseCase>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");
app.UseHttpsRedirection();
app.UsePersonEndpoints();
app.UsePersonLookupEndpoints();
app.Run();
public abstract partial class Program;