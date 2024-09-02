using CO.CDP.DataSharing.WebApi.Model;

namespace CO.CDP.DataSharing.WebApi;

public interface IPdfGenerator
{
    byte[] GenerateBasicInformationPdf(BasicInformation basicInformation);
}
