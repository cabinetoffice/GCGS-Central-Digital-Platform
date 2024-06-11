using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class UpdateBuyerInformationUseCase(IOrganisationRepository organisationRepository)
    : IUseCase<(Guid organisationId, UpdateBuyerInformation buyerInformation), bool>
{
    public async Task<bool> Execute((Guid organisationId, UpdateBuyerInformation buyerInformation) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
            ?? throw new UpdateBuyerInformationException.UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        switch (command.buyerInformation.Type)
        {
            case BuyerInformationPatchType.BuyerOrganisationType:
                // TODO: update BuyerOrganisationType
                break;
            case BuyerInformationPatchType.DevolvedRegulation:
                // TODO: update DevolvedRegulation
                break;
            default:
                break;
        }

        return await Task.FromResult(true);
    }

    public abstract class UpdateBuyerInformationException(string message, Exception? cause = null)
        : Exception(message, cause)
    {
        public class UnknownOrganisationException(string message, Exception? cause = null) : Exception(message, cause);
    }
}