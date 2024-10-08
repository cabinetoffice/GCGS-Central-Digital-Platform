using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetPersonInvitesUseCase(IPersonInviteRepository personInviteRepository, IMapper mapper)
    : IUseCase<Guid, IEnumerable<PersonInviteModel>>
{
    public async Task<IEnumerable<PersonInviteModel>> Execute(Guid organisationId)
    {
        var personInvites = await personInviteRepository.FindByOrganisation(organisationId);

        personInvites = personInvites.Where(pi => pi.Person == null);

        var personInviteModels = mapper.Map<IEnumerable<PersonInviteModel>>(personInvites);

        return personInviteModels;
    }
}