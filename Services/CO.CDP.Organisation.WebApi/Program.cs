using System.Reflection;
using CO.CDP.Authentication;
using CO.CDP.Authentication.Authorization;
using CO.CDP.AwsServices;
using CO.CDP.Configuration.Assembly;
using CO.CDP.Configuration.ForwardedHeaders;
using CO.CDP.Configuration.Helpers;
using CO.CDP.GovUKNotify;
using CO.CDP.MQ;
using CO.CDP.Organisation.WebApi;
using CO.CDP.Organisation.WebApi.Api;
using CO.CDP.Organisation.WebApi.AutoMapper;
using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Interfaces;
using CO.CDP.OrganisationInformation.Persistence.Repositories;
using CO.CDP.WebApi.Foundation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.AspNetCore.Authorization;
using Announcement = CO.CDP.Organisation.WebApi.Model.Announcement;
using ConnectedEntity = CO.CDP.Organisation.WebApi.Model.ConnectedEntity;
using ConnectedEntityLookup = CO.CDP.Organisation.WebApi.Model.ConnectedEntityLookup;
using MouSignature = CO.CDP.Organisation.WebApi.Model.MouSignature;
using Organisation = CO.CDP.Organisation.WebApi.Model.Organisation;
using OrganisationJoinRequest = CO.CDP.Organisation.WebApi.Model.OrganisationJoinRequest;
using Person = CO.CDP.Organisation.WebApi.Model.Person;
using SupplierInformation = CO.CDP.Organisation.WebApi.Model.SupplierInformation;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureForwardedHeaders();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.DocumentOrganisationApi(builder.Configuration); });
builder.Services.AddAutoMapper(typeof(WebApiToPersistenceProfile));

var connectionString = ConnectionStringHelper.GetConnectionString(builder.Configuration, "OrganisationInformationDatabase");
builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).MapEnums().Build());
builder.Services.AddHealthChecks().AddNpgSql(sp => sp.GetRequiredService<NpgsqlDataSource>());
builder.Services.AddDbContext<OrganisationInformationContext>((sp, o) => o.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));

builder.Services.AddScoped<IIdentifierService, IdentifierService>();
builder.Services.AddScoped<IOrganisationRepository, DatabaseOrganisationRepository>();
builder.Services.AddScoped<IOrganisationPartiesRepository, DatabaseOrganisationPartiesRepository>();
builder.Services.AddScoped<IConnectedEntityRepository, DatabaseConnectedEntityRepository>();
builder.Services.AddScoped<IPersonRepository, DatabasePersonRepository>();
builder.Services.AddScoped<IPersonInviteRepository, DatabasePersonInviteRepository>();
builder.Services.AddScoped<IAuthenticationKeyRepository, DatabaseAuthenticationKeyRepository>();
builder.Services.AddScoped<IOrganisationJoinRequestRepository, DatabaseOrganisationJoinRequestRepository>();
builder.Services.AddScoped<IShareCodeRepository, DatabaseShareCodeRepository>();
builder.Services.AddScoped<IAnnouncementRepository, DatabaseAnnouncementRepository>();
builder.Services.AddScoped<IOrganisationHierarchyRepository, OrganisationHierarchyRepository>();
builder.Services.AddScoped<IUseCase<AssignOrganisationIdentifier, bool>, AssignIdentifierUseCase>();
builder.Services.AddScoped<IUseCase<RegisterOrganisation, Organisation>, RegisterOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<Guid, Organisation?>, GetOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<Guid, IEnumerable<Review>>, GetReviewsUseCase>();
builder.Services.AddScoped<IUseCase<Organisation?>, GetMyOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<OrganisationQuery, Organisation?>, LookupOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<OrganisationSearchQuery, IEnumerable<OrganisationSearchResult>>, SearchOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<OrganisationSearchByPponQuery, (IEnumerable<OrganisationSearchByPponResult> Results, int TotalCount)>, SearchOrganisationByPponUseCase>();
builder.Services.AddScoped<IUseCase<OrganisationsByOrganisationEmailQuery, IEnumerable<OrganisationSearchResult>>, FindOrganisationByOrganisationEmailUseCase>();
builder.Services.AddScoped<IUseCase<OrganisationsByAdminEmailQuery, IEnumerable<OrganisationSearchResult>>, FindOrganisationByAdminEmailUseCase>();
builder.Services.AddScoped<IUseCase<PaginatedOrganisationQuery, Tuple<IEnumerable<OrganisationDto>, int>>, GetOrganisationsUseCase>();
builder.Services.AddScoped<IUseCase<Guid, SupplierInformation?>, GetSupplierInformationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid), ConnectedEntity?>, GetConnectedEntityUseCase>();
builder.Services.AddScoped<IUseCase<Guid, IEnumerable<ConnectedEntityLookup>>, GetConnectedEntitiesUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, UpdateOrganisation), bool>, UpdateOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, UpdateBuyerInformation), bool>, UpdateBuyerInformationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, UpdateSupplierInformation), bool>, UpdateSupplierInformationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, RegisterConnectedEntity), bool>, RegisterConnectedEntityUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid, UpdateConnectedEntity), bool>, UpdateConnectedEntityUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid), DeleteConnectedEntityResult>, DeleteConnectedEntityUseCase>();
builder.Services.AddScoped<IUseCase<Guid, IEnumerable<Person>>, GetPersonsUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, RemovePersonFromOrganisation), bool>, RemovePersonFromOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, InvitePersonToOrganisation), bool>, InvitePersonToOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid, UpdateInvitedPersonToOrganisation), bool>, UpdateInvitedPersonToOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid, UpdatePersonToOrganisation), bool>, UpdatePersonToOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<Guid, IEnumerable<PersonInviteModel>>, GetPersonInvitesUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid), bool>, RemovePersonInviteFromOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, SupportUpdateOrganisation), bool>, SupportUpdateOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<Guid, IEnumerable<CO.CDP.Organisation.WebApi.Model.AuthenticationKey>>, GetAuthenticationKeyUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, RegisterAuthenticationKey), bool>, RegisterAuthenticationKeyUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, string), bool>, RevokeAuthenticationKeyUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, CreateOrganisationJoinRequest), OrganisationJoinRequest>, CreateOrganisationJoinRequestUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, OrganisationJoinRequestStatus?), IEnumerable<JoinRequestLookUp>>, GetOrganisationJoinRequestUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid, UpdateJoinRequest), bool>, UpdateJoinRequestUseCase>();
builder.Services.AddScoped<IUseCase<ProvideFeedbackAndContact, bool>, ProvideFeedbackAndContactUseCase>();
builder.Services.AddScoped<IUseCase<ContactUs, bool>, ContactUsUseCase>();
builder.Services.AddScoped<IUseCase<Guid, CO.CDP.Organisation.WebApi.Model.BuyerInformation?>, GetBuyerInformationUseCase>();
builder.Services.AddScoped<IUseCase<Guid, OrganisationParties?>, GetOrganisationPartiesUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, RemoveOrganisationParty), bool>, RemoveOrganisationPartyUseCase>();
builder.Services.AddScoped<IUseCase<Guid, IEnumerable<MouSignature>>, GetOrganisationMouSignaturesUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, Guid), MouSignature>, GetOrganisationMouSignatureUseCase>();
builder.Services.AddScoped<IUseCase<Guid, MouSignatureLatest>, GetOrganisationMouSignatureLatestUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, SignMouRequest), bool>, SignOrganisationMouUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, AddOrganisationParty), bool>, AddOrganisationPartyUseCase>();
builder.Services.AddScoped<IUseCase<CO.CDP.Organisation.WebApi.Model.Mou>, GetLatestMouUseCase>();
builder.Services.AddScoped<IUseCase<Guid, CO.CDP.Organisation.WebApi.Model.Mou>, GetMouUseCase>();
builder.Services.AddScoped<IUseCase<(Guid, UpdateOrganisationParty), bool>, UpdateOrganisationPartyUseCase>();
builder.Services.AddScoped<IUseCase<GetAnnouncementQuery, IEnumerable<Announcement>>, GetAnnouncementsUseCase>();
builder.Services.AddScoped<IUseCase<(Guid organisationId, string role), IEnumerable<Person>>, GetPersonsInRoleUseCase>();
builder.Services.AddScoped<IUseCase<CreateParentChildRelationshipRequest, CreateParentChildRelationshipResult>, CreateParentChildRelationshipUseCase>();
builder.Services.AddScoped<IUseCase<Guid, GetChildOrganisationsResponse>, GetChildOrganisationsUseCase>();
builder.Services.AddScoped<ISupersedeChildOrganisationUseCase, SupersedeChildOrganisationUseCase>();
builder.Services.AddScoped<IUseCase<Guid, GetParentOrganisationsResponse>, GetParentOrganisationsUseCase>();

builder.Services.AddScoped<IAuthorizationHandler, OrganisationScopeAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ApiKeyScopeAuthorizationHandler>();

builder.Services.AddGovUKNotifyApiClient(builder.Configuration);
builder.Services.AddProblemDetails();

builder.Services.AddJwtBearerAndApiKeyAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddOrganisationAuthorization();

if ((Assembly.GetEntryAssembly().IsRunAs("CO.CDP.Organisation.WebApi")) ||
    (Assembly.GetEntryAssembly().IsRunAs("testhost")))
{
    builder.Services
        .AddAwsConfiguration(builder.Configuration)
        .AddLoggingConfiguration(builder.Configuration)
        .AddAmazonCloudWatchLogsService()
        .AddCloudWatchSerilog(builder.Configuration)
        .AddAwsSqsService()
        .AddOutboxSqsPublisher<OrganisationInformationContext>(
            builder.Configuration,
            enableBackgroundServices: Assembly.GetEntryAssembly().IsRunAs("CO.CDP.Organisation.WebApi"),
            notificationChannel: "organisation_information_outbox")
        .AddSqsDispatcher(
            EventDeserializer.Deserializer,
            enableBackgroundServices: Assembly.GetEntryAssembly().IsRunAs("CO.CDP.Organisation.WebApi"),
            (services) => { services.AddScoped<ISubscriber<PponGenerated>, PponGeneratedSubscriber>(); },
            (services, dispatcher) => { dispatcher.Subscribe<PponGenerated>(services); }
        );
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
    app.UseHsts();
}

app.UseStatusCodePages();

app.MapHealthChecks("/health").AllowAnonymous();
app.UseAuthentication();
app.UseAuthorization();
app.UseOrganisationEndpoints();
app.UseGlobalEndpoints();


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

app.MapGroup("/organisations")
    .UseOrganisationPartiesEndpoints()
    .WithTags("Organisation - Parties");

app.MapGroup("/feeback")
    .UseFeedbackEndpoints()
    .WithTags("Feedback - provide feedback");

app.MapGroup("/organisations")
    .UseOrganisationMouEndpoints()
    .WithTags("Organisation - MOUs");

if (app.Configuration.GetValue<bool>("BuyerParentChildRelationship"))
{
    app.MapGroup("/organisations")
        .UseOrganisationHierarchyEndpoints()
        .WithTags("Organisation - Hierarchy");
}

if (app.Configuration.GetValue<bool>("SearchRegistryPpon"))
{
    app.MapGroup("/organisation")
        .useSearchRegistryOfPpon()
        .WithTags("Organisation - Search Registry Of Ppon");
}

app.MapGroup("/mou")
    .UseMouEndpoints()
    .WithTags("Mou");

app.Run();
public abstract partial class Program;
