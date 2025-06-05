using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetSharedDataUseCase(
    OrganisationInformation.Persistence.IShareCodeRepository shareCodeRepository,
    IMapper mapper)
    : IUseCase<string, SupplierInformation?>
{
    public async Task<SupplierInformation?> Execute(string sharecode)
    {
        var sharedConsent = await shareCodeRepository.GetByShareCode(sharecode)
                            ?? throw new ShareCodeNotFoundException(Constants.ShareCodeNotFoundExceptionMessage);

        var supplierInformation = mapper.Map<SupplierInformation>(sharedConsent,
            opt => { opt.Items["RootSource"] = sharedConsent; });

        if (sharedConsent.Organisation.Type == OrganisationType.InformalConsortium)
        {
            await AddConsortiumOrganisationSharedConsent(sharecode, supplierInformation);
        }

        return supplierInformation;
    }

    private async Task AddConsortiumOrganisationSharedConsent(string parentShareCode, SupplierInformation supplierInformation)
    {
        var partiesShareCodes = await shareCodeRepository.GetConsortiumOrganisationsShareCode(parentShareCode);

        foreach (var partyShareCode in partiesShareCodes)
        {
            var partySharedConsent = await shareCodeRepository.GetByShareCode(partyShareCode)
                                ?? throw new ShareCodeNotFoundException(Constants.ShareCodeNotFoundExceptionMessage);

            var partySupplierInfo = mapper.Map<SupplierInformation>(partySharedConsent,
                opt => { opt.Items["RootSource"] = partySharedConsent; });

            supplierInformation.SupplierInformationData.AnswerSets.AddRange(partySupplierInfo.SupplierInformationData.AnswerSets);
            supplierInformation.SupplierInformationData.Questions.AddRange(partySupplierInfo.SupplierInformationData.Questions);
            supplierInformation.AssociatedPersons.AddRange(partySupplierInfo.AssociatedPersons);
            supplierInformation.AdditionalEntities.AddRange(partySupplierInfo.AdditionalEntities);
            supplierInformation.AdditionalParties.Add(new OrganisationReference
            {
                Id = partySharedConsent.Organisation.Guid,
                Name = partySharedConsent.Organisation.Name,
                Roles = partySharedConsent.Organisation.Roles,
                Uri = null,
                ShareCode = new OrganisationReferenceShareCode
                {
                    Value = partyShareCode,
                    SubmittedAt = partySharedConsent.SubmittedAt!.Value
                }
            });
        }
    }
}