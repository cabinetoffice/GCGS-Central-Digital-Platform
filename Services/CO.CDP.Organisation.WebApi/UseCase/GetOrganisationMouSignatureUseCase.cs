using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetOrganisationMouSignatureUseCase(IOrganisationRepository organisationRepository, IMapper mapper)
    : IUseCase<(Guid organisationId, Guid mouSignatureId), Model.MouSignature>
{
    public async Task<Model.MouSignature> Execute((Guid organisationId, Guid mouSignatureId) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
                           ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        return await organisationRepository.GetMouSignature(organisation.Id, command.mouSignatureId)
            .AndThen(mapper.Map<Model.MouSignature>);
    }
}