using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation;
using Persistence = CO.CDP.OrganisationInformation.Persistence;
using OrganisationJoinRequest = CO.CDP.Organisation.WebApi.Model.OrganisationJoinRequest;
using Person = CO.CDP.OrganisationInformation.Persistence.Person;
using AutoMapper;
using CO.CDP.Authentication;
using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class CreateOrganisationJoinRequestUseCase(
    Persistence.IOrganisationRepository organisationRepository,
    Persistence.IPersonRepository personRepository,
    Persistence.IOrganisationJoinRequestRepository organisationJoinRequestRepository,
    Func<Guid> guidFactory,
    IMapper mapper,
    IConfiguration configuration,
    IGovUKNotifyApiClient govUKNotifyApiClient,
    ILogger<CreateOrganisationJoinRequestUseCase> logger)
    : IUseCase<(Guid organisationId, CreateOrganisationJoinRequest createOrganisationJoinRequestCommand), OrganisationJoinRequest>
{
    public CreateOrganisationJoinRequestUseCase(
        Persistence.IOrganisationRepository organisationRepository,
        Persistence.IPersonRepository personRepository,
        Persistence.IOrganisationJoinRequestRepository organisationJoinRequestRepository,
        IMapper mapper,
        IConfiguration configuration,
        IGovUKNotifyApiClient govUKNotifyApiClient,
        ILogger<CreateOrganisationJoinRequestUseCase> logger
    ) : this(organisationRepository, personRepository, organisationJoinRequestRepository, Guid.NewGuid, mapper, configuration, govUKNotifyApiClient, logger)
    {

    }

    public async Task<OrganisationJoinRequest> Execute((Guid organisationId, CreateOrganisationJoinRequest createOrganisationJoinRequestCommand) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
                           ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var person = await personRepository.Find(command.createOrganisationJoinRequestCommand.PersonId)
                           ?? throw new UnknownPersonException($"Unknown person {command.createOrganisationJoinRequestCommand.PersonId}.");

        await GuardPersonIsNotAlreadyAdded(organisation, person);

        var joinRequest = await organisationJoinRequestRepository.FindByOrganisationAndPerson(organisation.Guid, person.Guid);

        if (joinRequest != null)
        {
            if (joinRequest.Status == OrganisationJoinRequestStatus.Pending)
            {
                return mapper.Map<OrganisationJoinRequest>(joinRequest);
            }

            if (joinRequest.Status == OrganisationJoinRequestStatus.Rejected)
            {
                joinRequest.Status = OrganisationJoinRequestStatus.Pending;
            }
        }
        else
        {
            joinRequest = CreateOrganisationJoinRequest(organisation, person);
        }

        organisationJoinRequestRepository.Save(joinRequest);

        await NotifyUserRequestSent(organisation: organisation, person: person);
        await NotifyOrgAdminsOfApprovalRequest(organisation: organisation, person: person);

        OrganisationJoinRequest request = mapper.Map<OrganisationJoinRequest>(joinRequest);
        request.RequestCreated = true;

        return request;
    }

    private Persistence.OrganisationJoinRequest CreateOrganisationJoinRequest(
        Persistence.Organisation organisation,
        Person person
    )
    {
        var organisationJoinRequest = new Persistence.OrganisationJoinRequest
        {
            Guid = guidFactory(),
            Organisation = organisation,
            Person = person,
            Status = OrganisationJoinRequestStatus.Pending
        };

        return organisationJoinRequest;
    }

    private async Task NotifyOrgAdminsOfApprovalRequest(Persistence.Organisation organisation, Person person)
    {
        var baseAppUrl = configuration.GetValue<string>("OrganisationAppUrl") ?? "";
        var templateId = configuration.GetValue<string>("GOVUKNotify:RequestToJoinNotifyOrgAdminTemplateId") ?? "";

        var missingConfigs = new List<string>();

        if (string.IsNullOrEmpty(baseAppUrl)) missingConfigs.Add("OrganisationAppUrl");
        if (string.IsNullOrEmpty(templateId)) missingConfigs.Add("GOVUKNotify:RequestToJoinNotifyOrgAdminTemplateId");

        if (missingConfigs.Count != 0)
        {
            logger.LogError(new Exception("Unable to send email to organisation admins"), $"Missing configuration keys: {string.Join(", ", missingConfigs)}. Unable to send email to organisation admins.");
            return;
        }

        var requestLink = new Uri(new Uri(baseAppUrl), $"/organisation/{organisation.Guid}/users/user-summary").ToString();

        var organisationAdminUsers = await organisationRepository.FindOrganisationPersons(organisation.Guid, [Constants.OrganisationPersonScope.Admin]);

        foreach (var adminPerson in organisationAdminUsers)
        {
            var emailRequest = new EmailNotificationRequest
            {
                EmailAddress = adminPerson.Person.Email,
                TemplateId = templateId,
                Personalisation = new Dictionary<string, string>
                {
                    { "org_name", organisation.Name },
                    { "request_link", requestLink },
                    { "first_name", adminPerson.Person.FirstName },
                    { "last_name", adminPerson.Person.LastName },
                    { "requester_first_name", person.FirstName },
                    { "requester_last_name", person.LastName }
                }
            };

            try
            {
                await govUKNotifyApiClient.SendEmail(emailRequest);
            }
            catch
            {
                return;
            }
        }
    }

    private async Task NotifyUserRequestSent(Persistence.Organisation organisation, Person person)
    {
        var templateId = configuration.GetValue<string>("GOVUKNotify:RequestToJoinConfirmationEmailTemplateId") ?? "";

        var missingConfigs = new List<string>();

        if (string.IsNullOrEmpty(templateId)) missingConfigs.Add("GOVUKNotify:RequestToJoinConfirmationEmailTemplateId");

        if (missingConfigs.Count != 0)
        {
            logger.LogError(new Exception("Unable to send email to buyer user"), $"Missing configuration keys: {string.Join(", ", missingConfigs)}. Unable to send email to buyer user.");
            return;
        }

        var emailRequest = new EmailNotificationRequest
        {
            EmailAddress = person.Email,
            TemplateId = templateId,
            Personalisation = new Dictionary<string, string>
            {
                { "org_name", organisation.Name },
                { "first_name", person.FirstName },
                { "last_name", person.LastName }
            }
        };

        try
        {
            await govUKNotifyApiClient.SendEmail(emailRequest);
        }
        catch
        {
            return;
        }
    }

    private async Task GuardPersonIsNotAlreadyAdded(Persistence.Organisation organisation, Person person)
    {
        var matchingOrganisationPerson = await organisationRepository.FindOrganisationPerson(organisation.Guid, person.Guid);

        if (matchingOrganisationPerson != null)
        {
            throw new PersonAlreadyAddedToOrganisationException(
                $"Person {person.Guid} already added to organisation {organisation.Guid}.");
        }
    }
}