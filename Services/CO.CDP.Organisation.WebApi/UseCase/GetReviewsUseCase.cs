using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetReviewsUseCase(IOrganisationRepository organisationRepository, IMapper mapper) : IUseCase<Guid, IEnumerable<Model.Review>>
{
    public async Task<IEnumerable<Model.Review>> Execute(Guid organisationId)
    {
        var organisation = await organisationRepository.Find(organisationId)
                           ?? throw new UnknownOrganisationException($"Unknown organisation {organisationId}.");

        return [ mapper.Map<Review>(organisation) ];
    }
}