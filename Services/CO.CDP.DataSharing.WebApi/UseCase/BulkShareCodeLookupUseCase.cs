using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class BulkShareCodeLookupUseCase(
    IShareCodeRepository shareCodeRepository,
    IMapper mapper)
    : IUseCase<BulkShareCodeLookupRequest, IEnumerable<BulkShareCodeLookupResult>>
{
    public async Task<IEnumerable<BulkShareCodeLookupResult>> Execute(BulkShareCodeLookupRequest request)
    {
        var results = new List<BulkShareCodeLookupResult>();

        var sharedConsents = (await shareCodeRepository.GetByShareCodes(request.ShareCodes.ToList()))
            .Where(s => s.ShareCode != null)
            .ToDictionary(s => s.ShareCode!);

        foreach (var shareCode in request.ShareCodes)
        {
            sharedConsents.TryGetValue(shareCode, out var sharedConsent);

            var partySupplierInfo = mapper.Map<Model.SupplierInformation>(sharedConsent,
                opt => { opt.Items["RootSource"] = sharedConsent; });

            results.Add(sharedConsent is null
                ? new BulkShareCodeLookupResult
                {
                    ShareCode = shareCode,
                    IsValid = false,
                    OrganisationId = null,
                    OrganisationName = null,
                    SubmittedAt = null,
                    Identifier = null!,
                    AdditionalIdentifiers = [],
                    Address = null!,
                    AdditionalAddresses = [],
                    ContactPoint = null!
                }
                : new BulkShareCodeLookupResult
                {
                    ShareCode = shareCode,
                    IsValid = true,
                    OrganisationId = sharedConsent.Organisation.Guid,
                    OrganisationName = sharedConsent.Organisation.Name,
                    SubmittedAt = sharedConsent.SubmittedAt,
                    Identifier = partySupplierInfo.Identifier,
                    AdditionalIdentifiers = partySupplierInfo.AdditionalIdentifiers,
                    Address = partySupplierInfo.Address,
                    AdditionalAddresses = partySupplierInfo.AdditionalAddresses,
                    ContactPoint = partySupplierInfo.ContactPoint
                });
        }

        return results;
    }
}
