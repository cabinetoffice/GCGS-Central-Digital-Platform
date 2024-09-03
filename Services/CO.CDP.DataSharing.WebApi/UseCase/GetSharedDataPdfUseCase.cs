using CO.CDP.DataSharing.WebApi.DataService;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetSharedDataPdfUseCase(
    IShareCodeRepository shareCodeRepository,
    IPdfGenerator pdfGenerator, IDataService dataService)
    : IUseCase<string, byte[]?>
{
    public async Task<byte[]?> Execute(string sharecode)
    {
        var sharedConsent = await shareCodeRepository.GetByShareCode(sharecode);
        if (sharedConsent == null)
        {
            throw new SharedConsentNotFoundException("Shared Consent not found.");
        }

        var sharedSupplierInfo = await dataService.GetSharedSupplierInformationAsync(sharedConsent);

        var pdfBytes = pdfGenerator.GenerateBasicInformationPdf(sharedSupplierInfo);

        return pdfBytes;
    }

}