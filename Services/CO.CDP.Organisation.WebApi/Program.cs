using System.Reflection;
using CO.CDP.Authentication;
using CO.CDP.AwsServices;
using CO.CDP.Configuration.Assembly;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.Configuration.Helpers;
using CO.CDP.GovUKNotify;
using CO.CDP.MQ;
using CO.CDP.MQ.Hosting;
using CO.CDP.Organisation.WebApi.Api;
using CO.CDP.Organisation.WebApi.AutoMapper;
using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.Organisation.WebApi.Extensions;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.EntityFrameworkCore;
using ConnectedEntity = CO.CDP.Organisation.WebApi.Model.ConnectedEntity;
using ConnectedEntityLookup = CO.CDP.Organisation.WebApi.Model.ConnectedEntityLookup;
using Organisation = CO.CDP.Organisation.WebApi.Model.Organisation;
using Person = CO.CDP.Organisation.WebApi.Model.Person;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureForwardedHeaders();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.DocumentOrganisationApi(builder.Configuration); });
builder.Services.AddHealthChecks()
    .AddNpgSql(ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase"));
builder.Services.AddAutoMapper(typeof(WebApiToPersistenceProfile));

builder.Services
    .AddAwsConfiguration(builder.Configuration)
    .AddAwsSqsService()
    .AddSqsPublisher()
    .AddSqsDispatcher(
        EventDeserializer.Deserializer,
        (services) =>
        {
            services.AddScoped<ISubscriber<PponGenerated>, PponGeneratedSubscriber>();
        },
        (services, dispatcher) =>
        {
            dispatcher.Subscribe<PponGenerated>(services);
        }
    );
if (Assembly.GetEntryAssembly().IsRunAs("CO.CDP.Organisation.WebApi"))
{
    builder.Services.AddHostedService<DispatcherBackgroundService>();
}
builder.Services.AddDbContext<OrganisationInformationContext>(o =>
    o.UseNpgsql(ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase")));
builder.Services.AddScoped<IOrganisationRepository, DatabaseOrganisationRepository>();
builder.Services.AddScoped<IConnectedEntityRepository, DatabaseConnectedEntityRepository>();
builder.Services.AddScoped<IPersonRepository, DatabasePersonRepository>();
builder.Services.AddScoped<IPersonInviteRepository, DatabasePersonInviteRepository>();
builder.Services.AddScoped<IAuthenticationKeyRepository, DatabaseAuthenticationKeyRepository>();
builder.Services.AddScoped<IUseCase<AssignOrganisationIdentifier, bool>, AssignIdentifierUseCase>();
builder.Services.AddScoped<IUseCase<RegisterOrganisation, Organisation>, RegisterOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<Guid, Organisation?>, GetOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<Organisation?>, GetMyOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<OrganisationQuery, Organisation?>, LookupOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<string, IEnumerable<Organisation>>, GetOrganisationsUseCase>();
builder.Services.AddScoped<IUseCase<Guid, SupplierInformation?>, GetSupplierInformationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid), ConnectedEntity?>, GetConnectedEntityUseCase>();
builder.Services.AddScoped<IUseCase<Guid, IEnumerable<ConnectedEntityLookup>>, GetConnectedEntitiesUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, UpdateOrganisation), bool>, UpdateOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, UpdateBuyerInformation), bool>, UpdateBuyerInformationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, UpdateSupplierInformation), bool>, UpdateSupplierInformationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, RegisterConnectedEntity), bool>, RegisterConnectedEntityUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid, UpdateConnectedEntity), bool>, UpdateConnectedEntityUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, DeleteSupplierInformation), bool>, DeleteSupplierInformationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid, DeleteConnectedEntity), bool>, DeleteConnectedEntityUseCase>();
builder.Services.AddScoped<IUseCase<Guid, IEnumerable<Person>>, GetPersonsUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, RemovePersonFromOrganisation), bool>, RemovePersonFromOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, InvitePersonToOrganisation), PersonInvite>, InvitePersonToOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid, UpdateInvitedPersonToOrganisation), bool>, UpdateInvitedPersonToOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid, UpdatePersonToOrganisation), bool>, UpdatePersonToOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<Guid, IEnumerable<PersonInviteModel>>, GetPersonInvitesUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid), bool>, RemovePersonInviteFromOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<Guid, IEnumerable<CO.CDP.Organisation.WebApi.Model.AuthenticationKey>>, GetAuthenticationKeyUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, RegisterAuthenticationKey), bool>, RegisterAuthenticationKeyUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, RevokeAuthenticationKey), bool>, RevokeAuthenticationKeyUseCase>();

builder.Services.AddOrganisationProblemDetails();

builder.Services.AddJwtBearerAndApiKeyAuthentication(builder.Configuration, builder.Environment);
//builder.Services.AddAuthorization();
builder.Services.AddOrganisationAuthorization();
builder.Services.AddGovUKNotifyApiClient(builder.Configuration);

if (Assembly.GetEntryAssembly().IsRunAs("CO.CDP.Organisation.WebApi"))
{
    builder.Services
        .AddAwsConfiguration(builder.Configuration)
        .AddLoggingConfiguration(builder.Configuration)
        .AddAmazonCloudWatchLogsService()
        .AddCloudWatchSerilog();
}

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

app.MapGroup("/organisations")
    .UsePersonsEndpoints()
    .WithTags("Organisation - Persons");

app.MapGroup("/organisations")
    .UseManageApiKeyEndpoints()
    .WithTags("Organisation - Manage Api Keys");

app.Run();
public abstract partial class Program;