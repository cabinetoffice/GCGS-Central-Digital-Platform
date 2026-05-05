using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

/// <summary>
/// Resends an existing invite by extending its lifespan and re-sending the notification email.
/// The invite GUID is preserved so the caller's cooldown keying remains valid.
/// </summary>
public class ResendPersonInviteUseCase(
    IOrganisationRepository organisationRepository,
    IPersonInviteRepository personInviteRepository,
    IGovUKNotifyApiClient govUkNotifyApiClient,
    IConfiguration configuration)
    : IUseCase<(Guid organisationId, Guid personInviteId), bool>
{
    public async Task<bool> Execute((Guid organisationId, Guid personInviteId) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
                           ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var invite = await personInviteRepository.Find(command.personInviteId)
                     ?? throw new UnknownInvitedPersonException($"Unknown invite {command.personInviteId}.");

        if (invite.OrganisationId != organisation.Id)
            throw new UnknownInvitedPersonException(
                $"Invite {command.personInviteId} does not belong to organisation {command.organisationId}.");

        invite.ExpiresOn = null;
        invite.InviteSentOn = DateTimeOffset.UtcNow;
        personInviteRepository.Save(invite);

        var baseAppUrl = configuration.GetValue<string>("OrganisationAppUrl")
                         ?? throw new InvalidOperationException("Missing configuration key: OrganisationAppUrl");

        var templateId = configuration.GetValue<string>("GOVUKNotify:PersonInviteEmailTemplateId")
                         ?? throw new InvalidOperationException(
                             "Missing configuration key: GOVUKNotify:PersonInviteEmailTemplateId.");

        Uri inviteLink = new Uri(new Uri(baseAppUrl), $"organisation-invite/{invite.Guid}");

        await govUkNotifyApiClient.SendEmail(new EmailNotificationRequest
        {
            EmailAddress = invite.Email,
            TemplateId = templateId,
            Personalisation = new Dictionary<string, string>
            {
                { "org_name", organisation.Name },
                { "first_name", invite.FirstName },
                { "last_name", invite.LastName },
                { "invite_link", inviteLink.ToString() }
            }
        });

        return true;
    }
}