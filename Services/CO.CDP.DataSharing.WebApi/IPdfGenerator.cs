using CO.CDP.DataSharing.WebApi.Model;

namespace CO.CDP.DataSharing.WebApi;

public interface IPdfGenerator
{
    Stream GenerateBasicInformationPdf(SharedSupplierInformation supplierInformation);
}
