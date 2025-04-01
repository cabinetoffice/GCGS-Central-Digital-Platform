using Amazon.Runtime.Internal.Transform;
using CO.CDP.Authentication;
using CO.CDP.AwsServices;
using CO.CDP.DataSharing.WebApi.DataService;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net.Mime;
using static CO.CDP.Authentication.Constants;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetSharedDataFileUseCase(
    IPdfGenerator pdfGenerator,
    IDataService dataService,
    IClaimService claimService,
    IFileHostManager fileHostManager,
    IShareCodeRepository shareCodeRepository)
    : IUseCase<string, SharedDataFile?>
{
    public async Task<SharedDataFile?> Execute(string sharecode)
    {
        List<SharedSupplierInformation> data = [];
        Dictionary<String, DateTimeOffset?> shareCodeList = [] ;

        var sharedSupplierInfo = await dataService.GetSharedSupplierInformationAsync(sharecode);
        if (sharedSupplierInfo == null) return null;

        data.Add(sharedSupplierInfo);
        if (sharedSupplierInfo.OrganisationType == OrganisationInformation.OrganisationType.InformalConsortium)
        {
            var shareCodes = await shareCodeRepository.GetConsortiumOrganisationsShareCode(sharecode);
            
            foreach (var sc in shareCodes)
            {
                var scdetails = await dataService.GetSharedSupplierInformationAsync(sc);
                data.Add(scdetails);
                shareCodeList.Add(scdetails.Sharecode, scdetails.SharecodeSubmittedAt);
            }
        }

        if (!await claimService.HaveAccessToOrganisation(
            sharedSupplierInfo.OrganisationId,
            [OrganisationPersonScope.Admin, OrganisationPersonScope.Editor, OrganisationPersonScope.Viewer],
            [PersonScope.SupportAdmin]))
        {
            throw new UserUnauthorizedException();
        }

        var pdfStream = pdfGenerator.GenerateBasicInformationPdf(data, shareCodeList);

        var fileName = $"{sharecode}.pdf";
        var contentType = MediaTypeNames.Application.Pdf;
        byte[] content = [];

        var docs = data.SelectMany(x => x.AttachedDocuments);

        if (docs.Any())
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

                foreach (var attachedDocument in docs)
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