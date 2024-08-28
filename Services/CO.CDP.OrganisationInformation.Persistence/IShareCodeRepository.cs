using CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.OrganisationInformation.Persistence;
public interface IShareCodeRepository : IDisposable
{
    Task<IEnumerable<SharedConsent>> GetShareCodesAsync(Guid organisationId);
    Task<SharedConsent?> GetSharedConsentDraftAsync(Guid formId, Guid organisationId);
    Task<SharedConsent?> GetByShareCode(string sharecode);    
    Task<SharedConsentDetails?> GetShareCodeDetailsAsync(Guid organisationId, string shareCode);
    Task<Boolean?> GetShareCodeVerifyAsync(string formVersionId, string shareCode);
}