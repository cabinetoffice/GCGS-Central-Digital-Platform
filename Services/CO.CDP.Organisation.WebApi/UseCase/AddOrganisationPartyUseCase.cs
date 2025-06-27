using CO.CDP.Authentication;
using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class AddOrganisationPartyUseCase(
    IOrganisationRepository orgRepo,
    IShareCodeRepository shareCodeRepo,
    IOrganisationPartiesRepository orgPartiesRepo,
    IGovUKNotifyApiClient govUKNotifyApiClient,
    IConfiguration configuration,
    IClaimService claimService,
    IPersonRepository personRepository,
    ILogger<AddOrganisationPartyUseCase> logger
    ) : IUseCase<(Guid, AddOrganisationParty), bool>
{
    public async Task<bool> Execute((Guid, AddOrganisationParty) command)
    {
        (Guid organisationId, AddOrganisationParty addParty) = command;

        var parentOrganisation = await orgRepo.Find(organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {organisationId}.");

        var childOrganisation = await orgRepo.Find(addParty.OrganisationPartyId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {addParty.OrganisationPartyId}.");

        int? sharedConsentId = null;

        if (!string.IsNullOrWhiteSpace(addParty.ShareCode))
        {
            var sharedConsents = await shareCodeRepo.GetShareCodesAsync(addParty.OrganisationPartyId);

            var sharedConsent = sharedConsents.FirstOrDefault(s =>
                    string.Equals(s.ShareCode, addParty.ShareCode, StringComparison.InvariantCultureIgnoreCase));

            if (sharedConsent == null
                || sharedConsent.SubmissionState != OrganisationInformation.Persistence.Forms.SubmissionState.Submitted
                || sharedConsent.OrganisationId != childOrganisation.Id)
            {
                throw new OrganisationShareCodeInvalid(addParty.ShareCode);
            }

            sharedConsentId = sharedConsent.Id;
        }

        await orgPartiesRepo.Save(
            new OrganisationInformation.Persistence.OrganisationParty
            {
                ParentOrganisationId = parentOrganisation.Id,
                ChildOrganisationId = childOrganisation.Id,
                OrganisationRelationship = (OrganisationInformation.Persistence.OrganisationRelationship)addParty.OrganisationRelationship,
                SharedConsentId = sharedConsentId,
            });

        if (sharedConsentId.HasValue)
        {
            await SendNotificationEmail(parentOrganisation, childOrganisation);
        }

        return true;
    }

    private async Task SendNotificationEmail(
        OrganisationInformation.Persistence.Organisation parentOrganisation,
        OrganisationInformation.Persistence.Organisation childOrganisation)
    {
        var templateId = configuration.GetValue<string>("GOVUKNotify:ConsortiumOrganisationAddedEmailTemplateId") ?? "";
        var orgAdmins = await orgRepo.FindOrganisationPersons(parentOrganisation.Guid, [Constants.OrganisationPersonScope.Admin]);

        var userUrn = claimService.GetUserUrn()
                        ?? throw new UnknownPersonException("Ensure the token is valid and contains the necessary claims.");

        var user = await personRepository.FindByUrn(userUrn)
                        ?? throw new UnknownPersonException($"Unknown person {userUrn}.");

        var userName = $"{user.FirstName} {user.LastName}";

        var emailNotificationRequests = orgAdmins.Select(p => new EmailNotificationRequest
        {
            EmailAddress = p.Person.Email,
            TemplateId = templateId,
            Personalisation = new Dictionary<string, string>
                {
                    { "org_name", childOrganisation.Name },
                    { "first_name", p.Person.FirstName },
                    { "last_name", p.Person.LastName },
                    { "consortium_name", parentOrganisation.Name },
                    { "person_who_added_it", userName }
                }
        });

        var orgContactEmail = parentOrganisation.ContactPoints?.FirstOrDefault()?.Email;
        if (orgContactEmail != null)
        {
            emailNotificationRequests = emailNotificationRequests.Append(new EmailNotificationRequest
            {
                EmailAddress = orgContactEmail,
                TemplateId = templateId,
                Personalisation = new Dictionary<string, string>
                {
                    { "org_name", childOrganisation.Name },
                    { "first_name", "organisation" },
                    { "last_name", "owner" },
                    { "consortium_name", parentOrganisation.Name },
                    { "person_who_added_it", userName }
                }
            });
        }

        var emailTasks = emailNotificationRequests.Select(async request =>
        {
            try
            {
                await govUKNotifyApiClient.SendEmail(request);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to send consortium organisation - {childOrganisation.Name} added email to {request.EmailAddress}");
            }
        });

        await Task.WhenAll(emailTasks);
    }
}