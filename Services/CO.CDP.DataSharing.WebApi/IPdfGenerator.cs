using CO.CDP.DataSharing.WebApi.Model;

namespace CO.CDP.DataSharing.WebApi;

public interface IPdfGenerator
{
    Stream GenerateBasicInformationPdf(List<SharedSupplierInformation> supplierInformations, Dictionary<String, DateTimeOffset?> shareCodes);
}
