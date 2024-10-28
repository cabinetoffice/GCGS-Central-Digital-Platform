using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
namespace CO.CDP.Organisation.WebApi.UseCase;

public class InvitePersonToOrganisationUseCase(
    IOrganisationRepository organisationRepository,
    IPersonInviteRepository personInviteRepository,
    IGovUKNotifyApiClient govUKNotifyApiClient,
    IConfiguration configuration,
    Func<Guid> guidFactory)
    : IUseCase<(Guid organisationId, InvitePersonToOrganisation invitePersonData), PersonInvite>
{
    public InvitePersonToOrganisationUseCase(
        IOrganisationRepository organisationRepository,
        IPersonInviteRepository personInviteRepository,
        IGovUKNotifyApiClient govUKNotifyApiClient,
        IConfiguration configuration
    ) : this(organisationRepository, personInviteRepository, govUKNotifyApiClient, configuration, Guid.NewGuid)
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

        var isInviteEmailUnique = await personInviteRepository.IsInviteEmailUniqueWithinOrganisation(command.organisationId, command.invitePersonData.Email);
        if (!isInviteEmailUnique)
        {
            throw new DuplicateInviteEmailForOrganisationException($"A user with this email address has already been invited to your organisation.");
        }

        var personInvite = CreatePersonInvite(command.invitePersonData, organisation);

        personInviteRepository.Save(personInvite);

        var baseAppUrl = configuration.GetValue<string>("OrganisationAppUrl")
                            ?? throw new Exception("Missing configuration key: OrganisationAppUrl");


        var templateId = configuration.GetValue<string>("GOVUKNotify:PersonInviteEmailTemplateId")
                            ?? throw new Exception("Missing configuration key: GOVUKNotify:PersonInviteEmailTemplateId.");

        Uri baseUri = new Uri(baseAppUrl);
        Uri inviteLink = new Uri(baseUri, $"organisation-invite/{personInvite.Guid}");

        var emailRequest = new EmailNotificationRequest
        {
            EmailAddress = personInvite.Email,
            TemplateId = templateId,
            Personalisation = new Dictionary<string, string> {
                                        { "org_name", organisation.Name},
                                        { "first_name", personInvite.FirstName},
                                        { "last_name", personInvite.LastName},
                                        { "invite_link", inviteLink.ToString()} }
        };

        await govUKNotifyApiClient.SendEmail(emailRequest);

        return personInvite;
    }

    private PersonInvite CreatePersonInvite(
        InvitePersonToOrganisation command,
        CO.CDP.OrganisationInformation.Persistence.Organisation organisation
    )
    {
        var personInvite = new PersonInvite
        {
            Guid = guidFactory(),
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            Organisation = organisation,
            Scopes = command.Scopes
        };

        return personInvite;
    }
}