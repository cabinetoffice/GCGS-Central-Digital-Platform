using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class InvitePersonToOrganisationUseCase(
    IOrganisationRepository organisationRepository,
    IPersonInviteRepository personInviteRepository,
    IMapper mapper,
    Func<Guid> guidFactory)
    : IUseCase<(Guid organisationId, InvitePersonToOrganisation invitePersonData), PersonInvite>
{
    public InvitePersonToOrganisationUseCase(
        IOrganisationRepository organisationRepository,
        IPersonInviteRepository personInviteRepository,
        IMapper mapper
    ) : this(organisationRepository, personInviteRepository, mapper, Guid.NewGuid)
    {

    }

    public async Task<PersonInvite> Execute((Guid organisationId, InvitePersonToOrganisation invitePersonData) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
                           ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var personInvite = CreatePersonInvite(command.invitePersonData, organisation);

        personInvite = EmailPersonInvite(personInvite);

        personInviteRepository.Save(personInvite);

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
            Scopes = command.Scopes,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        return personInvite;
    }

    private PersonInvite EmailPersonInvite(PersonInvite personInvite)
    {
        // TODO: Need to send out GOV Notify email to the user
        // This may need to be set elsewhere... We may use SQS to schedule the email to be sent
        // When the invite is emailed then update this field: personInvite.InviteSentOn;

        return personInvite;
    }
}