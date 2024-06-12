using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class GetSupplierInformationUseCase(IOrganisationRepository organisationRepository, IMapper mapper)
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

        if (supplierInfo.CompletedVat)
        {
            var vatIdentifier = organisation.Identifiers.FirstOrDefault(i => i.Scheme == "VAT");
            if (vatIdentifier != null) supplierInfo.VatNumber = vatIdentifier.IdentifierId;
        }

        return supplierInfo;
    }
}