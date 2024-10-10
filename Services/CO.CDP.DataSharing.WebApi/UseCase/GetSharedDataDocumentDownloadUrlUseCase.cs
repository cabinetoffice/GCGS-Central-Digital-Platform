using CO.CDP.AwsServices;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetSharedDataDocumentDownloadUrlUseCase(
    IShareCodeRepository shareCodeRepository,
    IFileHostManager fileHostManager)
    : IUseCase<(string, string), string?>
{
    public async Task<string?> Execute((string, string) command)
    {
        var (sharecode, documentId) = command;

        var exists = await shareCodeRepository.ShareCodeDocumentExistsAsync(sharecode, documentId);
        if (exists)
        {
            var url =  await fileHostManager.GeneratePresignedUrl(documentId, 10080);
            return url;
        }

        return null;
    }
}