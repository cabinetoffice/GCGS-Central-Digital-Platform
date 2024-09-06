using CO.CDP.DataSharing.WebApi.Model;

namespace CO.CDP.DataSharing.WebApi.DataService;

public interface IDataService
{
    Task<SharedSupplierInformation> GetSharedSupplierInformationAsync(string shareCode);
}