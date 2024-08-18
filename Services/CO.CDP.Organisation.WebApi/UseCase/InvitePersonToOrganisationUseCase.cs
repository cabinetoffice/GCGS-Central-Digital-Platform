using AutoMapper;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class InvitePersonToOrganisationUseCase(
    IOrganisationRepository organisationRepository,
    IPersonInviteRepository personInviteRepository,
    IMapper mapper,
    Func<Guid> guidFactory)
    : IUseCase<InvitePersonToOrganisation, PersonInvite>
{
    private readonly List<string> _defaultScopes = ["ADMIN", "RESPONDER"];

    public async Task<PersonInvite> Execute(InvitePersonToOrganisation command)
    {
        var organisation = await organisationRepository.Find(command.OrganisationId);

        var personInvite = CreatePersonInvite(command, organisation);

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
}