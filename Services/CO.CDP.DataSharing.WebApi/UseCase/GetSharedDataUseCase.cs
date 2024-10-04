using AutoMapper;
using CO.CDP.Authentication;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using SharedConsent = CO.CDP.OrganisationInformation.Persistence.Forms.SharedConsent;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetSharedDataUseCase(
    IShareCodeRepository shareCodeRepository,
    IOrganisationRepository organisationRepository,
    IClaimService claimService,
    IMapper mapper)
    : IUseCase<string, SupplierInformation?>
{
    public async Task<SupplierInformation?> Execute(string sharecode)
    {
        var organisationId = claimService.GetOrganisationId();
        if (organisationId == null || await shareCodeRepository.OrganisationShareCodeExistsAsync(organisationId.Value, sharecode) == false)
        {
            throw new UserUnauthorizedException();
        }

        var sharedConsent = await shareCodeRepository.GetByShareCode(sharecode)
                            ?? throw new ShareCodeNotFoundException(Constants.ShareCodeNotFoundExceptionMessage);

        var associatedPersons = await AssociatedPersons(sharedConsent);
        var additionalEntities = await AdditionalEntities(sharedConsent);
        var details = await GetDetails(sharedConsent);

        return mapper.Map<SupplierInformation>(sharedConsent, opts =>
        {
            opts.Items["AssociatedPersons"] = associatedPersons;
            opts.Items["AdditionalParties"] = new List<OrganisationReference>();
            opts.Items["AdditionalEntities"] = additionalEntities;

            opts.Items["Details"] = details;
        });
    }

    private async Task<Details> GetDetails(SharedConsent sharedConsent)
    {
        var legalForm = await organisationRepository.GetLegalForm(sharedConsent.OrganisationId);

        return new Details
        {
            LegalForm = legalForm != null ? mapper.Map<LegalForm>(legalForm) : null
        };
    }

    private async Task<ICollection<AssociatedPerson>> AssociatedPersons(SharedConsent sharedConsent)
    {
        var associatedPersons = await organisationRepository.GetConnectedIndividualTrusts(sharedConsent.OrganisationId);
        return associatedPersons.Select(x => new AssociatedPerson
        {
            Id = x.Guid,
            Name = string.Format($"{x.IndividualOrTrust?.FirstName} {x.IndividualOrTrust?.LastName}"),
            Relationship = x.IndividualOrTrust?.Category.ToString() ?? string.Empty,
            Uri = null,
            Roles = []
        }).ToList();
    }

    private async Task<ICollection<OrganisationReference>> AdditionalEntities(SharedConsent sharedConsent)
    {
        var additionalEntities = await organisationRepository.GetConnectedOrganisations(sharedConsent.OrganisationId);
        return additionalEntities.Select(x => new OrganisationReference
        {
            Id = x.Guid,
            Name = x.Organisation?.Name ?? string.Empty,
            Roles = [],
            Uri = null
        }).ToList();
    }
}