using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetOrganisationMouSignatureUseCase(IOrganisationRepository organisationRepository, IMapper mapper)
    : IUseCase<(Guid organisationId, Guid mouId), Model.MouSignature>
{
    public async Task<Model.MouSignature> Execute((Guid organisationId, Guid mouId) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
                           ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

           return await organisationRepository.GetMouSignatures(organisation.Id, command.mouId)
            .AndThen(mapper.Map<Model.MouSignature>);
    }
}