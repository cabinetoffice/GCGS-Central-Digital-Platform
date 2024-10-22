using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdateJoinRequestUseCase(
    IOrganisationRepository organisationRepository,
    IOrganisationJoinRequestRepository requestRepository,
    IPersonRepository personRepository,
    IGovUKNotifyApiClient govUKNotifyApiClient,
    IConfiguration configuration,
    ILogger<UpdateJoinRequestUseCase> logger)
    : IUseCase<(Guid organisationId, Guid joinRequestId, UpdateJoinRequest updateJoinRequest), bool>
{
    public async Task<bool> Execute((Guid organisationId, Guid joinRequestId, UpdateJoinRequest updateJoinRequest) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var joinRequest = await requestRepository.Find(command.joinRequestId, command.organisationId)
            ?? throw new UnknownOrganisationJoinRequestException($"Unknown organisation join request for org id {command.organisationId} or request id {command.joinRequestId}.");

        var person = await personRepository.Find(command.updateJoinRequest.ReviewedBy)
            ?? throw new UnknownPersonException($"Unknown person {command.updateJoinRequest.ReviewedBy}.");

        joinRequest.ReviewedById = person.Id;
        joinRequest.Status = command.updateJoinRequest.status;
        joinRequest.ReviewedOn = DateTimeOffset.UtcNow;

        if (command.updateJoinRequest.status == OrganisationInformation.OrganisationJoinRequestStatus.Accepted)
        {
            organisation.OrganisationPersons.Add(new OrganisationPerson
            {
                Person = joinRequest.Person!,
                Organisation = organisation,
                Scopes = []
            });

            organisationRepository.Save(organisation);
        }        

        requestRepository.Save(joinRequest);

        await NotifyAdminOfApprovalRequest(joinRequest);

        return await Task.FromResult(true);
    }

    private async Task NotifyAdminOfApprovalRequest(OrganisationInformation.Persistence.OrganisationJoinRequest organisationJoinRequest)
    {
        var baseAppUrl = configuration.GetValue<string>("OrganisationAppUrl") ?? "";
        var templateId = configuration.GetValue<string>("GOVUKNotify:RequestToJoinOrganisationDecisionTemplateId") ?? "";
        
        var missingConfigs = new List<string>();

        if (string.IsNullOrEmpty(baseAppUrl)) missingConfigs.Add("OrganisationAppUrl");
        if (string.IsNullOrEmpty(templateId)) missingConfigs.Add("GOVUKNotify:RequestToJoinOrganisationDecisionTemplateId");
        
        if (missingConfigs.Count != 0)
        {
            logger.LogError(new Exception("Unable to send an email"), $"Missing configuration keys: {string.Join(", ", missingConfigs)}. Unable to send an email.");
            return;
        }

        var orgLink = new Uri(new Uri(baseAppUrl), $"/organisation/{organisationJoinRequest.Organisation!.Guid}").ToString();

        var emailRequest = new EmailNotificationRequest
        {
            EmailAddress = organisationJoinRequest.Person!.Email,
            TemplateId = templateId,
            Personalisation = new Dictionary<string, string>
            {
                { "org_name", organisationJoinRequest.Organisation!.Name },
                { "decision",  organisationJoinRequest.Status.ToString().ToLower()},
                { "first_name", organisationJoinRequest.Person.FirstName},
                { "last_name", organisationJoinRequest.Person.LastName },
                { "org_link", orgLink  }
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