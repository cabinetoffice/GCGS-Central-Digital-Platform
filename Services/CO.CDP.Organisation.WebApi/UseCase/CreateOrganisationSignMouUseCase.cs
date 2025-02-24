using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.Organisation.WebApi.Model;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class SignOrganisationMouUseCase(
    Persistence.IOrganisationRepository organisationRepository,
    Persistence.IPersonRepository personRepository,
    IGovUKNotifyApiClient govUKNotifyApiClient,
    IConfiguration configuration,
    ILogger<SignOrganisationMouUseCase> logger
    )
    : IUseCase<(Guid organisationId, SignMouRequest signMouRequest), bool>
{
    public async Task<bool> Execute((Guid organisationId, SignMouRequest signMouRequest) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
                          ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var person = await personRepository.Find(command.signMouRequest.CreatedById)
                           ?? throw new UnknownPersonException($"Unknown person {command.signMouRequest.CreatedById}.");

        var mou = await organisationRepository.GetMou(command.signMouRequest.MouId)
                   ?? throw new UnknownMouException($"Unknown Mou {command.signMouRequest.MouId}.");


        Persistence.MouSignature mouSignature = new Persistence.MouSignature
        {
            MouId = mou.Id,
            Mou = mou,
            CreatedById = person.Id,
            CreatedBy = person,
            OrganisationId = organisation.Id,
            Organisation = organisation,
            Name = command.signMouRequest.Name,
            JobTitle = command.signMouRequest.JobTitle,
            SignatureGuid = Guid.NewGuid()
        };

        organisationRepository.SaveOrganisationMou(mouSignature);

        await NotifyCopyOfMoURequest(mouSignature, organisation);

        return await Task.FromResult(true);
    }

    private async Task NotifyCopyOfMoURequest(Persistence.MouSignature mouSignature, Persistence.Organisation organisation)
    {
        var baseAppUrl = configuration.GetValue<string>("OrganisationAppUrl") ?? "";
        var templateId = configuration.GetValue<string>("GOVUKNotify:MouDataSharingAgreementEmailTemplateId") ?? "";

        var missingConfigs = new List<string>();

        if (string.IsNullOrEmpty(baseAppUrl)) missingConfigs.Add("OrganisationAppUrl");
        if (string.IsNullOrEmpty(templateId)) missingConfigs.Add("GOVUKNotify:MouDataSharingAgreementEmailTemplateId");

        if (missingConfigs.Count != 0)
        {
            logger.LogError(new Exception("Unable to send MoU email"), $"Missing configuration keys: {string.Join(", ", missingConfigs)}. Unable to send MoU email to user.");
            return;
        }

        var requestLink = new Uri(new Uri(baseAppUrl), $"/mou-pdfs/{mouSignature.Mou.FilePath}").ToString();

        var emailRecipients = new List<string> { mouSignature.CreatedBy.Email };

        var orgContactEmail = organisation.ContactPoints?.FirstOrDefault()?.Email;
        if (orgContactEmail != null)
        {
            emailRecipients.Add(orgContactEmail);
        }

        var emailRequest = new EmailNotificationRequest
        {
            EmailAddress = mouSignature.CreatedBy.Email,
            TemplateId = templateId,
            Personalisation = new Dictionary<string, string>
                {   { "link", requestLink },
                    { "first_name", mouSignature.CreatedBy.FirstName },
                    { "last_name", mouSignature.CreatedBy.LastName },
                    { "job_title", mouSignature.JobTitle },
                    { "date", mouSignature.CreatedOn.ToString("dd MMM yyyy") },
                    { "time", mouSignature.CreatedOn.ToString("HH:mm") }
                }
        };

        try
        {
            foreach (var er in emailRecipients)
            {
                emailRequest.EmailAddress = er;
                await govUKNotifyApiClient.SendEmail(emailRequest);
            }            
        }
        catch
        {
            return;
        }

    }
}
