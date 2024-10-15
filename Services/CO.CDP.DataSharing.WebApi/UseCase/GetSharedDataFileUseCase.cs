using CO.CDP.Authentication;
using CO.CDP.AwsServices;
using CO.CDP.DataSharing.WebApi.DataService;
using CO.CDP.DataSharing.WebApi.Model;
using System.IO.Compression;
using System.Net.Mime;
using static CO.CDP.Authentication.Constants;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetSharedDataFileUseCase(
    IPdfGenerator pdfGenerator,
    IDataService dataService,
    IClaimService claimService,
    IFileHostManager fileHostManager)
    : IUseCase<string, SharedDataFile?>
{
    public async Task<SharedDataFile?> Execute(string sharecode)
    {
        var sharedSupplierInfo = await dataService.GetSharedSupplierInformationAsync(sharecode);
        if (sharedSupplierInfo == null) return null;

        if (!await claimService.HaveAccessToOrganisation(
            sharedSupplierInfo.OrganisationId,
            [OrganisationPersonScope.Admin, OrganisationPersonScope.Editor, OrganisationPersonScope.Viewer],
            [PersonScope.SupportAdmin]))
        {
            throw new UserUnauthorizedException();
        }

        var pdfStream = pdfGenerator.GenerateBasicInformationPdf(sharedSupplierInfo);

        var fileName = $"{sharecode}.pdf";
        var contentType = MediaTypeNames.Application.Pdf;
        byte[] content = [];

        if (sharedSupplierInfo.AttachedDocuments.Count > 0)
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var pdfInArchive = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                using (var pdfEntryStream = pdfInArchive.Open())
                {
                    pdfStream.Seek(0, SeekOrigin.Begin);
                    await pdfStream.CopyToAsync(pdfEntryStream);
                }

                foreach (var attachedDocument in sharedSupplierInfo.AttachedDocuments)
                {
                    var fileInArchive = archive.CreateEntry(attachedDocument, CompressionLevel.Optimal);
                    using (var entryStream = fileInArchive.Open())
                    {
                        using var attachedDocumentStream = await fileHostManager.DownloadFile(attachedDocument);
                        await attachedDocumentStream.CopyToAsync(entryStream);
                    }
                }
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            content = memoryStream.ToArray();
            contentType = MediaTypeNames.Application.Zip;
            fileName = $"{sharecode}.zip";
        }
        else
        {
            content = ((MemoryStream)pdfStream).ToArray();
        }

        return new SharedDataFile { Content = content, ContentType = contentType, FileName = fileName };
    }
}