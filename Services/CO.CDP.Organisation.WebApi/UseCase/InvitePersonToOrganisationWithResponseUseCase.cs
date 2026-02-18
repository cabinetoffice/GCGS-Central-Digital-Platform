using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

/// <summary>
/// Use case for inviting a person to an organisation that returns the created PersonInvite.
/// Used by service-to-service calls that need the invite GUID.
/// </summary>
public class InvitePersonToOrganisationWithResponseUseCase(
    IOrganisationRepository organisationRepository,
    IPersonInviteRepository personInviteRepository,
    IGovUKNotifyApiClient govUkNotifyApiClient,
    IConfiguration configuration,
    Func<Guid> guidFactory)
    : IUseCase<(Guid organisationId, InvitePersonToOrganisation invitePersonData), PersonInvite>
{
    public InvitePersonToOrganisationWithResponseUseCase(
        IOrganisationRepository organisationRepository,
        IPersonInviteRepository personInviteRepository,
        IGovUKNotifyApiClient govUkNotifyApiClient,
        IConfiguration configuration
    ) : this(organisationRepository, personInviteRepository, govUkNotifyApiClient, configuration, Guid.NewGuid)
    {
    }

    public async Task<PersonInvite> Execute((Guid organisationId, InvitePersonToOrganisation invitePersonData) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
                           ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var isEmailUnique = await organisationRepository.IsEmailUniqueWithinOrganisation(command.organisationId, command.invitePersonData.Email);
        if (!isEmailUnique)
        {
            throw new DuplicateEmailWithinOrganisationException($"A user with this email address already exists for your organisation.");
        }

        var existingInvites = await ExpireExistingPersonInvites(command.organisationId, command.invitePersonData.Email);
        var newInvite = CreatePersonInvite(command.invitePersonData, organisation);

        await personInviteRepository.SaveNewInvite(newInvite, existingInvites);

        var baseAppUrl = configuration.GetValue<string>("OrganisationAppUrl")
                            ?? throw new InvalidOperationException("Missing configuration key: OrganisationAppUrl");

        var templateId = configuration.GetValue<string>("GOVUKNotify:PersonInviteEmailTemplateId")
                            ?? throw new InvalidOperationException("Missing configuration key: GOVUKNotify:PersonInviteEmailTemplateId.");

        Uri baseUri = new Uri(baseAppUrl);
        Uri inviteLink = new Uri(baseUri, $"organisation-invite/{newInvite.Guid}");

        var emailRequest = new EmailNotificationRequest
        {
            EmailAddress = newInvite.Email,
            TemplateId = templateId,
            Personalisation = new Dictionary<string, string> {
                                        { "org_name", organisation.Name},
                                        { "first_name", newInvite.FirstName},
                                        { "last_name", newInvite.LastName},
                                        { "invite_link", inviteLink.ToString()} }
        };

        await govUkNotifyApiClient.SendEmail(emailRequest);

        return newInvite;
    }

    private async Task<IEnumerable<PersonInvite>> ExpireExistingPersonInvites(Guid organisationId, string email)
    {
        var now = DateTimeOffset.UtcNow;
        return (await personInviteRepository.FindPersonInviteByEmail(organisationId, email))
            .Select(personInvite => ExpireInvite(personInvite, now))
            .ToList();
    }

    private static PersonInvite ExpireInvite(PersonInvite personInvite, DateTimeOffset now)
    {
        if (personInvite.ExpiresOn == null)
        {
            personInvite.ExpiresOn = now;
        }

        return personInvite;
    }

    private PersonInvite CreatePersonInvite(
        InvitePersonToOrganisation command,
        OrganisationInformation.Persistence.Organisation organisation
    )
    {
        var personInvite = new PersonInvite
        {
            Guid = guidFactory(),
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            OrganisationId = organisation.Id,
            Scopes = command.Scopes
        };

        return personInvite;
    }
}
