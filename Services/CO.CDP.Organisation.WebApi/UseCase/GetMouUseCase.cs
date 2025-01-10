using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class GetMouUseCase(IOrganisationRepository organisationRepository, IMapper mapper)
    : IUseCase<Guid, Model.Mou>
{
    public async Task<Model.Mou> Execute(Guid mouId)
    {       
        var mou = await organisationRepository.GetMou(mouId)
            ?? throw new UnknownMouException($"No MOU found.");

       return mapper.Map<Model.Mou>(mou);
    }
}