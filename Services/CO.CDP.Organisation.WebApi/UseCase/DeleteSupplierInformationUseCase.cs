using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class DeleteSupplierInformationUseCase(IOrganisationRepository organisationRepository)
            : IUseCase<(Guid organisationId, DeleteSupplierInformation deleteSupplierInformation), bool>
{
    public async Task<bool> Execute((Guid organisationId, DeleteSupplierInformation deleteSupplierInformation) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        if (organisation.SupplierInfo == null)
        {
            throw new SupplierInfoNotExistException($"Supplier information for organisation {command.organisationId} not exist.");
        }

        switch (command.deleteSupplierInformation.Type)
        {
            case SupplierInformationDeleteType.TradeAssurance:
                if (command.deleteSupplierInformation.TradeAssuranceId == null)
                {
                    throw new InvalidUpdateSupplierInformationCommand("Missing trade assurance id.");
                }
                var tradeAssurance = organisation.SupplierInfo.TradeAssurances
                    .FirstOrDefault(t => t.Guid == command.deleteSupplierInformation.TradeAssuranceId);
                if (tradeAssurance != null)
                {
                    organisation.SupplierInfo.TradeAssurances.Remove(tradeAssurance);
                }
                break;

            default:
                throw new InvalidUpdateSupplierInformationCommand("Unknown supplier information delete type.");
        }

        organisationRepository.Save(organisation);

        return await Task.FromResult(true);
    }
}