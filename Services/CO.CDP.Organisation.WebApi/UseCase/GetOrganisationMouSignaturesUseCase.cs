using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetOrganisationMouSignaturesUseCase(IOrganisationRepository organisationRepository, IMapper mapper)
    : IUseCase<Guid, IEnumerable<Model.MouSignature>>
{
    public async Task<IEnumerable<Model.MouSignature>> Execute(Guid organisationId)
    {
        var organisation = await organisationRepository.Find(organisationId)
                           ?? throw new UnknownOrganisationException($"Unknown organisation {organisationId}.");

        var mouSignatures = await organisationRepository.GetMouSignatures(organisation.Id);

        return mapper.Map<IEnumerable<Model.MouSignature>>(mouSignatures);
    }
}