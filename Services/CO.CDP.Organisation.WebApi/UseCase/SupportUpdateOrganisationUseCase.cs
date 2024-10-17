using CO.CDP.GovUKNotify.Models;
using CO.CDP.GovUKNotify;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class SupportUpdateOrganisationUseCase(
    IOrganisationRepository organisationRepository,
    IPersonRepository personRepository,
    IGovUKNotifyApiClient govUKNotifyApiClient,
    IConfiguration configuration,
    ILogger<SupportUpdateOrganisationUseCase> logger)
    : IUseCase<(Guid organisationId, SupportUpdateOrganisation supportUpdateOrganisation), bool>
{
    public async Task<bool> Execute((Guid organisationId, SupportUpdateOrganisation supportUpdateOrganisation) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId) ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");
        var sendemail = false;

        switch (command.supportUpdateOrganisation.Type)
        {
            case SupportOrganisationUpdateType.Review:

                var personId = command.supportUpdateOrganisation.Organisation.ReviewedById;

                var person = await personRepository.Find(personId) ?? throw new UnknownPersonException($"Unknown person {personId}.");

                if (command.supportUpdateOrganisation.Organisation.Approved)
                {
                    organisation.ApprovedOn = DateTimeOffset.UtcNow;
                    organisation.Roles = organisation.PendingRoles;
                    organisation.PendingRoles = [];
                    sendemail = true;
                }

                organisation.ReviewedBy = person;
                organisation.ReviewComment = command.supportUpdateOrganisation.Organisation.Comment;

                break;
            default:
                throw new InvalidSupportUpdateOrganisationCommand("Unknown support update organisation command type.");
        }

        organisationRepository.Save(organisation);

        if (sendemail)
        {
            await NotifyBuyerApprovedRequest(organisation);
        }

        return true;
    }

    private async Task NotifyBuyerApprovedRequest(OrganisationInformation.Persistence.Organisation organisation)
    {
        var baseAppUrl = configuration.GetValue<string>("OrganisationAppUrl") ?? "";
        var templateId = configuration.GetValue<string>("GOVUKNotify:BuyerApprovedEmailTemplateId") ?? "";

        var missingConfigs = new List<string>();

        if (string.IsNullOrEmpty(baseAppUrl)) missingConfigs.Add("OrganisationAppUrl");
        if (string.IsNullOrEmpty(templateId)) missingConfigs.Add("GOVUKNotify:BuyerApprovedEmailTemplateId");

        if (missingConfigs.Count != 0)
        {
            logger.LogError(new Exception("Unable to send buyer approved email"), $"Missing configuration keys: {string.Join(", ", missingConfigs)}. Unable to send buyer approved email.");
            return;
        }

        var orgLink = new Uri(new Uri(baseAppUrl), $"/organisation/{organisation.Guid}").ToString();

        var orgPersons = await organisationRepository.FindOrganisationPersons(organisation.Guid);

        var adminPersons = orgPersons.Where(p => p.Scopes.Contains("ADMIN")).ToList();
        if (!adminPersons.Any())
        {
            logger.LogError(new Exception("Unable to send buyer approved email"), "Admin person not found");
            return;
        }

        var emailTasks = adminPersons.Select(async p =>
        {
            try
            {
                var emailRequest = new EmailNotificationRequest
                {
                    EmailAddress = p.Person.Email,
                    TemplateId = templateId,
                    Personalisation = new Dictionary<string, string>
                {
                    { "org_name", organisation.Name },
                    { "first_name", p.Person.FirstName },
                    { "last_name", p.Person.LastName },
                    { "org_link", orgLink }
                }
                };

                await govUKNotifyApiClient.SendEmail(emailRequest);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to send email to {p.Person.Email} for organisation {organisation.Name}");
            }
        });

        await Task.WhenAll(emailTasks);
    }
}