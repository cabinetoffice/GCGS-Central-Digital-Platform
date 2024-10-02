using AutoMapper;
using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.MQ;
using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using Person = CO.CDP.OrganisationInformation.Persistence.Person;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class RegisterOrganisationUseCase(
    IIdentifierService identifierService,
    IOrganisationRepository organisationRepository,
    IPersonRepository personRepository,
    IGovUKNotifyApiClient govUKNotifyApiClient,
    IPublisher publisher,
    IMapper mapper,
    IConfiguration configuration,
    ILogger<RegisterOrganisationUseCase> logger,
    Func<Guid> guidFactory)
    : IUseCase<RegisterOrganisation, Model.Organisation>
{
    private readonly List<string> _defaultScopes = ["ADMIN", "RESPONDER", "EDITOR"];

    public RegisterOrganisationUseCase(
        IIdentifierService identifierService,
        IOrganisationRepository organisationRepository,
        IPersonRepository personRepository,
        IGovUKNotifyApiClient govUKNotifyApiClient,
        IPublisher publisher,
        IMapper mapper,
        IConfiguration configuration,
        ILogger<RegisterOrganisationUseCase> logger)
        : this(identifierService,
              organisationRepository,
              personRepository,
              govUKNotifyApiClient,
              publisher,
              mapper,
              configuration,
              logger,
              Guid.NewGuid)
    {
    }

    public async Task<Model.Organisation> Execute(RegisterOrganisation command)
    {
        var person = await FindPerson(command);
        var organisation = CreateOrganisation(command, person);
        organisationRepository.Save(organisation);

        if (organisation.Roles.Contains(OrganisationInformation.PartyRole.Buyer))
        {
            await NotifyAdminOfApprovalRequest(organisation);
        }

        await publisher.Publish(mapper.Map<OrganisationRegistered>(organisation));
        return mapper.Map<Model.Organisation>(organisation);
    }

    private async Task<Person> FindPerson(RegisterOrganisation command)
    {
        Person? person = await personRepository.Find(command.PersonId);
        if (person == null)
        {
            throw new UnknownPersonException($"Unknown person {command.PersonId}.");
        }

        return person;
    }

    private OrganisationInformation.Persistence.Organisation CreateOrganisation(
        RegisterOrganisation command,
        Person person
    )
    {
        var organisation = MapRequestToOrganisation(command, person);
        organisation.OrganisationPersons.Add(new OrganisationPerson
        {
            Person = person,
            Organisation = organisation,
            Scopes = _defaultScopes
        });

        organisation.Identifiers
            .Where(i => i.Uri == null)
            .Select(i => i.Uri = identifierService.GetRegistryUri(command.Identifier.Scheme, command.Identifier.Id))
            .ToList();

        organisation.UpdateBuyerInformation();
        organisation.UpdateSupplierInformation();
        return organisation;
    }
    private async Task NotifyAdminOfApprovalRequest(OrganisationInformation.Persistence.Organisation organisation)
    {
        var baseAppUrl = configuration.GetValue<string>("OrganisationAppUrl") ?? "";
        var templateId = configuration.GetValue<string>("GOVUKNotify:RequestReviewApplicationEmailTemplateId") ?? "";
        var supportAdminEmailAddress = configuration.GetValue<string>("GOVUKNotify:SupportAdminEmailAddress") ?? "";

        var missingConfigs = new List<string>();

        if (string.IsNullOrEmpty(baseAppUrl)) missingConfigs.Add("OrganisationAppUrl");
        if (string.IsNullOrEmpty(templateId)) missingConfigs.Add("GOVUKNotify:RequestReviewApplicationEmailTemplateId");
        if (string.IsNullOrEmpty(supportAdminEmailAddress)) missingConfigs.Add("GOVUKNotify:SupportAdminEmailAddress");

        if (missingConfigs.Count != 0)
        {
            logger.LogError(new Exception("Unable to send email to support admin"), $"Missing configuration keys: {string.Join(", ", missingConfigs)}. Unable to send email to support admin.");
            return;
        }

        var requestLink = new Uri(new Uri(baseAppUrl), $"/support/organisation/{organisation.Guid}/approval").ToString();

        var emailRequest = new EmailNotificationRequest
        {
            EmailAddress = supportAdminEmailAddress,
            TemplateId = templateId,
            Personalisation = new Dictionary<string, string>
                {
                    { "org_name", organisation.Name },
                    { "request_link", requestLink }
                }
        };

        await govUKNotifyApiClient.SendEmail(emailRequest);
    }

    private OrganisationInformation.Persistence.Organisation MapRequestToOrganisation(
        RegisterOrganisation command,
        Person person
    ) =>
        mapper.Map<OrganisationInformation.Persistence.Organisation>(command, o =>
        {
            o.Items["Guid"] = guidFactory();
            o.Items["Tenant"] = new Tenant
            {
                Guid = guidFactory(),
                Name = command.Name,
                Persons = { person }
            };
        });
}