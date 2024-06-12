using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdateBuyerInformationUseCase(IOrganisationRepository organisationRepository)
                : IUseCase<(Guid organisationId, UpdateBuyerInformation updateBuyerInformation), bool>
{
    public async Task<bool> Execute((Guid organisationId, UpdateBuyerInformation updateBuyerInformation) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        if (organisation.BuyerInfo == null)
        {
            throw new BuyerInfoNotExistException($"Buyer information for organisation {command.organisationId} not exist.");
        }

        var updateObject = command.updateBuyerInformation.BuyerInformation;

        switch (command.updateBuyerInformation.Type)
        {
            case BuyerInformationUpdateType.BuyerOrganisationType:
                if (updateObject.BuyerType == null)
                {
                    throw new InvalidUpdateBuyerInformationCommand("Missing buyer type.");
                }
                organisation.BuyerInfo.BuyerType = updateObject.BuyerType;
                break;

            case BuyerInformationUpdateType.DevolvedRegulation:
                organisation.BuyerInfo.DevolvedRegulations = updateObject.DevolvedRegulations ?? [];
                break;

            default:
                throw new InvalidUpdateBuyerInformationCommand("Unknown buyer information update type.");
        }

        organisationRepository.Save(organisation);

        return await Task.FromResult(true);
    }
}