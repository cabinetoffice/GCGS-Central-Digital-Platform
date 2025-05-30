using CO.CDP.OrganisationInformation.Persistence.Forms;
using CO.CDP.OrganisationInformation.Persistence.NonEfEntities;

namespace CO.CDP.OrganisationInformation.Persistence;

public interface IShareCodeRepository : IDisposable
{
    Task<bool> ShareCodeDocumentExistsAsync(string shareCode, string documentId);
    Task<IEnumerable<SharedConsent>> GetShareCodesAsync(Guid organisationId);
    Task<SharedConsent?> GetSharedConsentDraftAsync(Guid formId, Guid organisationId);
    Task<SharedConsentNonEf?> GetByShareCode(string sharecode);
    Task<SharedConsentDetails?> GetShareCodeDetailsAsync(Guid organisationId, string shareCode);
    Task<bool?> GetShareCodeVerifyAsync(string formVersionId, string shareCode);
    Task<IEnumerable<string>> GetConsortiumOrganisationsShareCode(string shareCode);
}