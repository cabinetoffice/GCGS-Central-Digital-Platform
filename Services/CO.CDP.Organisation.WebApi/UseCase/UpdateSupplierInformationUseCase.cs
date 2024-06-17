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

            case SupplierInformationUpdateType.CompletedWebsiteAddress:
                organisation.SupplierInfo.CompletedWebsiteAddress = true;
                break;

            case SupplierInformationUpdateType.CompletedEmailAddress:
                organisation.SupplierInfo.CompletedEmailAddress = true;
                break;

            default:
                throw new InvalidUpdateSupplierInformationCommand("Unknown supplier information update type.");
        }

        organisationRepository.Save(organisation);

        return await Task.FromResult(true);
    }
}