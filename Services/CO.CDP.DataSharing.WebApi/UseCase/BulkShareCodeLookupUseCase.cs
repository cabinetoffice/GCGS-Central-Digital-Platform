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

        foreach (var shareCode in request.ShareCodes)
        {
            var sharedConsent = await shareCodeRepository.GetByShareCode(shareCode)
                                ?? throw new ShareCodeNotFoundException(Constants.ShareCodeNotFoundExceptionMessage);

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
