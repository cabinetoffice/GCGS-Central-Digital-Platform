using CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.OrganisationInformation.Persistence;

public interface IShareCodeRepository : IDisposable
{
    Task<bool> OrganisationShareCodeExistsAsync(Guid organisationId, string shareCode);
    Task<bool> ShareCodeDocumentExistsAsync(string shareCode, string documentId);
    Task<IEnumerable<SharedConsent>> GetShareCodesAsync(Guid organisationId);
    Task<SharedConsent?> GetSharedConsentDraftAsync(Guid formId, Guid organisationId);
    Task<SharedConsent?> GetByShareCode(string sharecode);
    Task<SharedConsentDetails?> GetShareCodeDetailsAsync(Guid organisationId, string shareCode);
    Task<bool?> GetShareCodeVerifyAsync(string formVersionId, string shareCode);
}