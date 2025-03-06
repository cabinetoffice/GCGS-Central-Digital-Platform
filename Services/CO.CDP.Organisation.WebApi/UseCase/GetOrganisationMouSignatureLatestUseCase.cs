using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class GetOrganisationMouSignatureLatestUseCase(IOrganisationRepository organisationRepository, IMapper mapper)
    : IUseCase<Guid, Model.MouSignatureLatest>
{
    public async Task<Model.MouSignatureLatest> Execute(Guid organisationId)
    {
        var organisation = await organisationRepository.Find(organisationId)
                           ?? throw new UnknownOrganisationException($"Unknown organisation {organisationId}.");

        var mouSignatures = await organisationRepository.GetMouSignatures(organisation.Id);

        if (mouSignatures == null || !mouSignatures.Any())
        {
            throw new UnknownMouException($"No MOU signatures found for organisation {organisationId}.");
        }

        var latestSignature = mouSignatures.OrderByDescending(m => m.CreatedOn).First();

        var latestMou = await organisationRepository.GetLatestMou()
            ?? throw new UnknownMouException($"No MOU found.");


        var mouSignatureLatestModel = mapper.Map<Model.MouSignatureLatest>(latestSignature);

        mouSignatureLatestModel.IsLatest = latestMou.Id == latestSignature.MouId;

        return mouSignatureLatestModel;
    }
}