using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdateBuyerInformationUseCase(IOrganisationRepository organisationRepository)
    : IUseCase<(Guid organisationId, UpdateBuyerInformation updateBuyerInformation), bool>
{

    public async Task<bool> Execute((Guid organisationId, UpdateBuyerInformation updateBuyerInformation) command)
    {

        var organisation = await organisationRepository.Find(command.organisationId)
            ?? throw new UpdateBuyerInformationException.UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        if (organisation.BuyerInfo == null)
        {
            throw new UpdateBuyerInformationException.BuyerInfoNotExistException($"Buyer information for organisation {command.organisationId} not exist.");
        }

        switch (command.updateBuyerInformation.Type)
        {
            case BuyerInformationUpdateType.BuyerOrganisationType:
                organisation.BuyerInfo.BuyerType = command.updateBuyerInformation.BuyerInformation.BuyerType;
                break;

            case BuyerInformationUpdateType.DevolvedRegulation:
                organisation.BuyerInfo.DevolvedRegulations = command.updateBuyerInformation.BuyerInformation.DevolvedRegulations ?? [];
                break;

            default:
                throw new UpdateBuyerInformationException.UnknownBuyerInformationUpdateTypeException("Unknown buyer information update type."); //Bad request
        }

        organisationRepository.Save(organisation);

        return await Task.FromResult(true);
    }

    public abstract class UpdateBuyerInformationException(string message, Exception? cause = null)
        : Exception(message, cause)
    {
        public class UnknownOrganisationException(string message, Exception? cause = null) : Exception(message, cause);
        public class BuyerInfoNotExistException(string message, Exception? cause = null) : Exception(message, cause);
        public class UnknownBuyerInformationUpdateTypeException(string message, Exception? cause = null) : Exception(message, cause);
    }

}