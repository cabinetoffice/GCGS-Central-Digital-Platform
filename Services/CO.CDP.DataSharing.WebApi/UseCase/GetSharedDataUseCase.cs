using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using SharedConsent = CO.CDP.OrganisationInformation.Persistence.Forms.SharedConsent;

namespace CO.CDP.DataSharing.WebApi.UseCase;

public class GetSharedDataUseCase(
    IShareCodeRepository shareCodeRepository,
    IOrganisationRepository organisationRepository,
    IMapper mapper)
    : IUseCase<string, SupplierInformation?>
{
    public async Task<SupplierInformation?> Execute(string sharecode)
    {
        var sharedConsent = await shareCodeRepository.GetByShareCode(sharecode)
                            ?? throw new SharedConsentNotFoundException("Shared Consent not found.");

        var associatedPersons = await AssociatedPersons(sharedConsent);
        var additionalEntities = await AdditionalEntities(sharedConsent);
        return mapper.Map<SupplierInformation>(sharedConsent, opts =>
        {
            opts.Items["AssociatedPersons"] = associatedPersons;
            opts.Items["AdditionalParties"] = new List<OrganisationReference>();
            opts.Items["AdditionalEntities"] = additionalEntities;
        });
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