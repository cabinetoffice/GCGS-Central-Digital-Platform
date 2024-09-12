using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.DataSharing.WebApi.DataService;

public class DataService(IShareCodeRepository shareCodeRepository, IConnectedEntityRepository connectedEntityRepository) : IDataService
{
    public async Task<SharedSupplierInformation> GetSharedSupplierInformationAsync(string shareCode)
    {
        var sharedConsent = await shareCodeRepository.GetByShareCode(shareCode)
                            ?? throw new ShareCodeNotFoundException(Constants.ShareCodeNotFoundExceptionMessage);

        return DataMappingFactory.MapToSharedSupplierInformation(sharedConsent, connectedEntityRepository);
    }
}