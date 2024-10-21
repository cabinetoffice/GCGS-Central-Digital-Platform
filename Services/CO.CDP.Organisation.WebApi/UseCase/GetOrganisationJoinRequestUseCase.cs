using AutoMapper;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetOrganisationJoinRequestUseCase(IOrganisationJoinRequestRepository requestRepository, IMapper mapper)
    : IUseCase<(Guid organisationId, OrganisationJoinRequestStatus? status), IEnumerable<Model.JoinRequestLookUp>>
{
    public async Task<IEnumerable<Model.JoinRequestLookUp>> Execute((Guid organisationId, OrganisationJoinRequestStatus? status) command)
    {
        var organisations = await requestRepository.FindByOrganisation(command.organisationId);

        if (command.status != null)
        {
            organisations = organisations.Where(o => o.Status == command.status);
        }        

        var result = mapper.Map<IEnumerable<Model.JoinRequestLookUp>>(organisations);

        return result;
    }
}