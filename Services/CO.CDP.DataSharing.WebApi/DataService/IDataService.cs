using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using SharedConsent = CO.CDP.OrganisationInformation.Persistence.Forms.SharedConsent;

namespace CO.CDP.DataSharing.WebApi.DataService;

public interface IDataService
{
    Task<SharedSupplierInformation> GetSharedSupplierInformationAsync(SharedConsent sharedConsent);
    BasicInformation MapToBasicInformation(Organisation organisation);
}
