using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdateSupplierInformationUseCase(IOrganisationRepository organisationRepository)
            : IUseCase<(Guid organisationId, UpdateSupplierInformation updateSupplierInformation), bool>
{
    public async Task<bool> Execute((Guid organisationId, UpdateSupplierInformation updateSupplierInformation) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        if (organisation.SupplierInfo == null)
        {
            throw new SupplierInfoNotExistException($"Supplier information for organisation {command.organisationId} not exist.");
        }

        var updateObject = command.updateSupplierInformation.SupplierInformation;

        switch (command.updateSupplierInformation.Type)
        {
            case SupplierInformationUpdateType.SupplierType:
                if (updateObject.SupplierType == null)
                {
                    throw new InvalidUpdateSupplierInformationCommand("Missing supplier type.");
                }
                organisation.SupplierInfo.SupplierType = updateObject.SupplierType;
                break;

            case SupplierInformationUpdateType.Vat:
                if (updateObject.HasVatNumber == null || (updateObject.HasVatNumber == true && string.IsNullOrWhiteSpace(updateObject.VatNumber)))
                {
                    throw new InvalidUpdateSupplierInformationCommand("Missing vat identifier.");
                }
                if (updateObject.HasVatNumber == true)
                {
                    organisation.Identifiers.Add(new OrganisationInformation.Persistence.Organisation.Identifier
                    {
                        IdentifierId = updateObject.VatNumber!,
                        Primary = false,
                        LegalName = organisation.Name,
                        Scheme = "VAT"
                    });
                }
                else
                {
                    var vatIdentifier = organisation.Identifiers.FirstOrDefault(i => i.Scheme == "VAT");
                    if (vatIdentifier != null) organisation.Identifiers.Remove(vatIdentifier);
                }
                organisation.SupplierInfo.CompletedVat = true;
                break;

            default:
                throw new InvalidUpdateSupplierInformationCommand("Unknown supplier information update type.");
        }

        organisationRepository.Save(organisation);

        return await Task.FromResult(true);
    }
}