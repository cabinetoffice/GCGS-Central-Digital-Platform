using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.GovUKNotify;
using GovukNotify.Models;
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

        var personInvite = CreatePersonInvite(command.invitePersonData, organisation);

        personInviteRepository.Save(personInvite);

        var templateId = configuration.GetValue<string>("GOVUKNotify:PersonInviteEmailTemplateId")
                            ?? throw new Exception("Missing configuration key: GOVUKNotify:PersonInviteEmailTemplateId.");

        var invitationLink = "https://www.GOV.UK.One.Login.com";
        var emailRequest = new EmailNotificationResquest
        {
            EmailAddress = personInvite.Email,
            TemplateId = templateId,
            Personalisation = new Dictionary<string, string> {
                                        { "org name", organisation.Name},
                                        { "first name", personInvite.FirstName},
                                        { "last name", personInvite.LastName},
                                        { "days", "7" },
                                        { "expiry date", personInvite.InviteSentOn.AddDays(6).ToString("dddd dd MMMM")},
                                        { "invitation link", invitationLink} }
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