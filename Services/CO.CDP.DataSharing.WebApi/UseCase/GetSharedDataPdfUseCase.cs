using CO.CDP.DataSharing.WebApi.DataService;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetSharedDataPdfUseCase(IPdfGenerator pdfGenerator, IDataService dataService)
    : IUseCase<string, byte[]?>
{
    public async Task<byte[]?> Execute(string sharecode)
    {
        var sharedSupplierInfo = await dataService.GetSharedSupplierInformationAsync(sharecode);

        var pdfBytes = pdfGenerator.GenerateBasicInformationPdf(sharedSupplierInfo);

        return pdfBytes;
    }
}