using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.DataSharing.WebApi.DataService;

public interface IDataService
{
    Task<SharedSupplierInformation> GetSharedSupplierInformationAsync(string shareCode);
    BasicInformation MapToBasicInformation(Organisation organisation);
}