using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class GetLatestMouUseCase(IOrganisationRepository organisationRepository, IMapper mapper)
    : IUseCase<Model.Mou>
{
    public async Task<Model.Mou> Execute()
    {       
        var latestMou = await organisationRepository.GetLatestMou()
            ?? throw new UnknownMouException($"No MOU found.");

       return mapper.Map<Model.Mou>(latestMou);
    }
}