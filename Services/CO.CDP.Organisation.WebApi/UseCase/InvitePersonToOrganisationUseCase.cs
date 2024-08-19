using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class InvitePersonToOrganisationUseCase(
    IOrganisationRepository organisationRepository,
    IPersonInviteRepository personInviteRepository,
    IMapper mapper,
    Func<Guid> guidFactory)
    : IUseCase<InvitePersonToOrganisation, PersonInvite>
{
    public async Task<PersonInvite> Execute(InvitePersonToOrganisation command)
    {
        var organisation = await organisationRepository.Find(command.OrganisationId);

        var personInvite = CreatePersonInvite(command, organisation);

        personInvite = EmailPersonInvite(personInvite);

        personInviteRepository.Save(personInvite);

        return personInvite;
    }

    private OrganisationInformation.Persistence.PersonInvite CreatePersonInvite(
        InvitePersonToOrganisation command,
        CO.CDP.OrganisationInformation.Persistence.Organisation organisation
    )
    {
        var personInvite = mapper.Map<OrganisationInformation.Persistence.PersonInvite>(command, o =>
        {
            o.Items["Guid"] = guidFactory();
        });

        personInvite.Organisation = organisation;
        return personInvite;
    }

    private OrganisationInformation.Persistence.PersonInvite EmailPersonInvite(PersonInvite personInvite)
    {
        // TODO: Need to send out Notify email to the user

        // This may need to be set elsewhere... We may use SQS to schedule the email to be sent
        personInvite.InviteSentOn = DateTimeOffset.UtcNow;

        return personInvite;
    }
}