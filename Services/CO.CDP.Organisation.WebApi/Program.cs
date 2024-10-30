using CO.CDP.Authentication;
using CO.CDP.AwsServices;
using CO.CDP.Configuration.Assembly;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.Configuration.Helpers;
using CO.CDP.GovUKNotify;
using CO.CDP.MQ;
using CO.CDP.MQ.Hosting;
using CO.CDP.Organisation.WebApi;
using CO.CDP.Organisation.WebApi.Api;
using CO.CDP.Organisation.WebApi.AutoMapper;
using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.WebApi.Foundation;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using ConnectedEntity = CO.CDP.Organisation.WebApi.Model.ConnectedEntity;
using ConnectedEntityLookup = CO.CDP.Organisation.WebApi.Model.ConnectedEntityLookup;
using Organisation = CO.CDP.Organisation.WebApi.Model.Organisation;
using OrganisationJoinRequest = CO.CDP.Organisation.WebApi.Model.OrganisationJoinRequest;
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
    .AddOutboxSqsPublisher<OrganisationInformationContext>()
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
    builder.Services.AddHostedService<OutboxProcessorBackgroundService>();
}
builder.Services.AddDbContext<OrganisationInformationContext>(o =>
    o.UseNpgsql(ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase")));
builder.Services.AddScoped<IIdentifierService, IdentifierService>();
builder.Services.AddScoped<IOrganisationRepository, DatabaseOrganisationRepository>();
builder.Services.AddScoped<IConnectedEntityRepository, DatabaseConnectedEntityRepository>();
builder.Services.AddScoped<IPersonRepository, DatabasePersonRepository>();
builder.Services.AddScoped<IPersonInviteRepository, DatabasePersonInviteRepository>();
builder.Services.AddScoped<IAuthenticationKeyRepository, DatabaseAuthenticationKeyRepository>();
builder.Services.AddScoped<IOrganisationJoinRequestRepository, DatabaseOrganisationJoinRequestRepository>();
builder.Services.AddScoped<IUseCase<AssignOrganisationIdentifier, bool>, AssignIdentifierUseCase>();
builder.Services.AddScoped<IUseCase<RegisterOrganisation, Organisation>, RegisterOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<Guid, Organisation?>, GetOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<Organisation?>, GetMyOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<OrganisationQuery, Organisation?>, LookupOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<PaginatedOrganisationQuery, IEnumerable<OrganisationExtended>>, GetOrganisationsUseCase>();
builder.Services.AddScoped<IUseCase<Guid, SupplierInformation?>, GetSupplierInformationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid), ConnectedEntity?>, GetConnectedEntityUseCase>();
builder.Services.AddScoped<IUseCase<Guid, IEnumerable<ConnectedEntityLookup>>, GetConnectedEntitiesUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, UpdateOrganisation), bool>, UpdateOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, UpdateBuyerInformation), bool>, UpdateBuyerInformationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, UpdateSupplierInformation), bool>, UpdateSupplierInformationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, RegisterConnectedEntity), bool>, RegisterConnectedEntityUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid, UpdateConnectedEntity), bool>, UpdateConnectedEntityUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid, DeleteConnectedEntity), bool>, DeleteConnectedEntityUseCase>();
builder.Services.AddScoped<IUseCase<Guid, IEnumerable<Person>>, GetPersonsUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, RemovePersonFromOrganisation), bool>, RemovePersonFromOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, InvitePersonToOrganisation), bool>, InvitePersonToOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid, UpdateInvitedPersonToOrganisation), bool>, UpdateInvitedPersonToOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid, UpdatePersonToOrganisation), bool>, UpdatePersonToOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<Guid, IEnumerable<PersonInviteModel>>, GetPersonInvitesUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid), bool>, RemovePersonInviteFromOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, SupportUpdateOrganisation), bool>, SupportUpdateOrganisationUseCase>();
builder.Services.AddGovUKNotifyApiClient(builder.Configuration);
builder.Services.AddScoped<IUseCase<Guid, IEnumerable<CO.CDP.Organisation.WebApi.Model.AuthenticationKey>>, GetAuthenticationKeyUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, RegisterAuthenticationKey), bool>, RegisterAuthenticationKeyUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, string), bool>, RevokeAuthenticationKeyUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, CreateOrganisationJoinRequest), OrganisationJoinRequest>, CreateOrganisationJoinRequestUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, OrganisationJoinRequestStatus?), IEnumerable<JoinRequestLookUp>>, GetOrganisationJoinRequestUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid, UpdateJoinRequest), bool>, UpdateJoinRequestUseCase>();
builder.Services.AddScoped<IUseCase<ProvideFeedbackAndContact, bool>, ProvideFeedbackAndContactUseCase>();
builder.Services.AddScoped<IUseCase<ContactUs, bool>, ContactUsUseCase>();

builder.Services.AddProblemDetails();

builder.Services.AddJwtBearerAndApiKeyAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddOrganisationAuthorization();

if (Assembly.GetEntryAssembly().IsRunAs("CO.CDP.Organisation.WebApi"))
{
    builder.Services
        .AddAwsConfiguration(builder.Configuration)
        .AddLoggingConfiguration(builder.Configuration)
        .AddAmazonCloudWatchLogsService()
        .AddCloudWatchSerilog(builder.Configuration);
}

var app = builder.Build();
app.UseForwardedHeaders();
app.UseErrorHandler(ErrorCodes.Exception4xxMap);

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
app.UseAuthentication();
app.UseAuthorization();
app.UseOrganisationEndpoints();


app.MapGroup("/support")
    .UseSupportEndpoints()
    .WithTags("Organisation - Support");

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

app.MapGroup("/feeback")
    .UseFeedbackEndpoints()
    .WithTags("Feedback - provide feedback");

app.Run();
public abstract partial class Program;