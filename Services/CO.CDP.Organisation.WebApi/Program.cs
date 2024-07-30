using CO.CDP.Authentication;
using CO.CDP.AwsServices;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.Organisation.WebApi.Api;
using CO.CDP.Organisation.WebApi.AutoMapper;
using CO.CDP.Organisation.WebApi.Extensions;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.EntityFrameworkCore;
using ConnectedEntity = CO.CDP.Organisation.WebApi.Model.ConnectedEntity;
using ConnectedEntityLookup = CO.CDP.Organisation.WebApi.Model.ConnectedEntityLookup;
using Organisation = CO.CDP.Organisation.WebApi.Model.Organisation;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureForwardedHeaders();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.DocumentOrganisationApi(); });
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("OrganisationInformationDatabase") ?? "");
builder.Services.AddAutoMapper(typeof(WebApiToPersistenceProfile));

builder.Services
    .AddAwsConfiguration(builder.Configuration)
    .AddAwsSqsService()
    .AddSqsPublisher();
builder.Services.AddDbContext<OrganisationInformationContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("OrganisationInformationDatabase") ?? ""));
builder.Services.AddScoped<IOrganisationRepository, DatabaseOrganisationRepository>();
builder.Services.AddScoped<IConnectedEntityRepository, DatabaseConnectedEntityRepository>();
builder.Services.AddScoped<IPersonRepository, DatabasePersonRepository>();
builder.Services.AddScoped<IUseCase<RegisterOrganisation, Organisation>, RegisterOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<Guid, Organisation?>, GetOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<OrganisationQuery, Organisation?>, LookupOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<string, IEnumerable<Organisation>>, GetOrganisationsUseCase>();
builder.Services.AddScoped<IUseCase<Guid, SupplierInformation?>, GetSupplierInformationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid), ConnectedEntity?>, GetConnectedEntityUseCase>();
builder.Services.AddScoped<IUseCase<Guid, IEnumerable<ConnectedEntityLookup>>, GetConnectedEntitiesUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, UpdateOrganisation), bool>, UpdateOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, UpdateBuyerInformation), bool>, UpdateBuyerInformationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, UpdateSupplierInformation), bool>, UpdateSupplierInformationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, RegisterConnectedEntity), bool>, RegisterConnectedEntityUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, DeleteSupplierInformation), bool>, DeleteSupplierInformationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid, DeleteConnectedEntity), bool>, DeleteConnectedEntityUseCase>();
builder.Services.AddOrganisationProblemDetails();

builder.Services.AddJwtBearerAndApiKeyAuthentication(builder.Configuration, builder.Environment);
//builder.Services.AddAuthorization();
builder.Services.AddOrganisationAuthorization();

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
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseStatusCodePages();

app.MapHealthChecks("/health").AllowAnonymous();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseOrganisationEndpoints();

app.MapGroup("/organisation")
    .UseOrganisationLookupEndpoints()
    .WithTags("Organisation - Lookup");

app.MapGroup("/organisations")
    .UseBuyerInformationEndpoints()
    .WithTags("Organisation - Buyer Information");

app.MapGroup("/organisations")
    .UseSupplierInformationEndpoints()
    .WithTags("Organisation - Supplier Information");

app.MapGroup("/organisations")
    .UseConnectedEntityEndpoints()
    .WithTags("Organisation - Connected Entity");


app.Run();
public abstract partial class Program;