using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetSupplierInformationUseCase(
    OrganisationInformation.Persistence.IOrganisationRepository organisationRepository, IMapper mapper)
    : IUseCase<Guid, SupplierInformation?>
{
    public async Task<SupplierInformation?> Execute(Guid command)
    {
        var organisation = await organisationRepository.Find(command);
        if (organisation == null || organisation.SupplierInfo == null)
        {
            return null;
        }

        var supplierInfo = mapper.Map<SupplierInformation>(organisation.SupplierInfo);
        supplierInfo.OrganisationName = organisation.Name;

        return supplierInfo;
    }
}