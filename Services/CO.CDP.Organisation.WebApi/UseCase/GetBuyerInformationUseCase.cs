using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetBuyerInformationUseCase(IOrganisationRepository organisationRepository, IMapper mapper)
    : IUseCase<Guid, BuyerInformation?>
{
    public async Task<BuyerInformation?> Execute(Guid command)
    {
        var organisation = await organisationRepository.Find(command);
        if (organisation == null || organisation.BuyerInfo == null)
        {
            return null;
        }

        return mapper.Map<BuyerInformation>(organisation.BuyerInfo);
    }
}