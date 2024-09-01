using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetSharedDataUseCase(
    IShareCodeRepository shareCodeRepository,
    IOrganisationRepository organisationRepository,
    IMapper mapper)
    : IUseCase<string, SupplierInformation?>
{
    public async Task<SupplierInformation?> Execute(string sharecode)
    {
        var sharedConsent = await shareCodeRepository.GetByShareCode(sharecode);
        if (sharedConsent == null)
        {
            throw new SharedConsentNotFoundException("Shared Consent not found.");
        }

        var result = mapper.Map<SupplierInformation>(sharedConsent);

        var associatedPersons = await organisationRepository.GetConnectedIndividualTrusts(sharedConsent.OrganisationId);
        result.AssociatedPersons = associatedPersons.Select(x => new AssociatedPerson()
        {
            Id = x.Guid,
            Name = string.Format($"{x.IndividualOrTrust?.FirstName} {x.IndividualOrTrust?.LastName}"),
            Relationship = x.IndividualOrTrust?.Category.ToString() ?? string.Empty,
            Uri = null,
            Roles = []
        }).ToList();

        var additionalEntities = await organisationRepository.GetConnectedOrganisations(sharedConsent.OrganisationId);
        result.AdditionalEntities = additionalEntities.Select(x => new OrganisationReference()
        {
            Id = x.Guid,
            Name = x.Organisation?.Name ?? string.Empty,
            Roles = [],
            Uri = null
        }).ToList();

        return result;
    }
}