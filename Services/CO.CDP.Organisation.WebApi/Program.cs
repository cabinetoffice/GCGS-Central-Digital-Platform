using CO.CDP.Organisation.Persistence;
using CO.CDP.Organisation.WebApi.Api;
using CO.CDP.Organisation.WebApi.AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using Organisation = CO.CDP.Organisation.WebApi.Model.Organisation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.DocumentOrganisationApi(); });

builder.Services.AddHealthChecks();
builder.Services.AddAutoMapper(typeof(WebApiToPersistenceProfile));

builder.Services.AddScoped<OrganisationContext>(_ => new OrganisationContext(builder.Configuration.GetConnectionString("OrganisationDatabase") ?? ""));
builder.Services.AddScoped<IOrganisationRepository, DatabaseOrganisationRepository>();
builder.Services.AddScoped<IUseCase<RegisterOrganisation, Organisation>, RegisterOrganisationUseCase>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");
app.UseHttpsRedirection();
app.UseOrganisationEndpoints();
app.Run();
public abstract partial class Program;