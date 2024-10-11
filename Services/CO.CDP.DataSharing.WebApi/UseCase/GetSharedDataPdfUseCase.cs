using CO.CDP.Authentication;
using CO.CDP.DataSharing.WebApi.DataService;
using CO.CDP.DataSharing.WebApi.Model;
using static CO.CDP.Authentication.Constants;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetSharedDataPdfUseCase(
    IPdfGenerator pdfGenerator,
    IDataService dataService,
    IClaimService claimService)
    : IUseCase<string, byte[]?>
{
    public async Task<byte[]?> Execute(string sharecode)
    {
        var sharedSupplierInfo = await dataService.GetSharedSupplierInformationAsync(sharecode);

        if (!await claimService.HaveAccessToOrganisation(
            sharedSupplierInfo.OrganisationId,
            [OrganisationPersonScope.Admin, OrganisationPersonScope.Editor, OrganisationPersonScope.Viewer],
            [PersonScope.SupportAdmin]))
        {
            throw new UserUnauthorizedException();
        }

        var pdfBytes = pdfGenerator.GenerateBasicInformationPdf(sharedSupplierInfo);

        return pdfBytes;
    }
}