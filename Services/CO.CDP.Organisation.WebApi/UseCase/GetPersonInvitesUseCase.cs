using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetPersonInvitesUseCase(IPersonInviteRepository personInviteRepository, IMapper mapper)
    : IUseCase<Guid, IEnumerable<PersonInviteModel>>
{
    public async Task<IEnumerable<PersonInviteModel>> Execute(Guid organisationId)
    {
        return await personInviteRepository.FindByOrganisation(organisationId)
            .AndThen(mapper.Map<IEnumerable<PersonInviteModel>>);
    }
}