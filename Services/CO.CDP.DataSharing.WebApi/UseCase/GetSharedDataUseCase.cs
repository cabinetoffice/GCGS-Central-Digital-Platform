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

        var associatedPersons = await organisationRepository.GetConnectedEntities(sharedConsent.OrganisationId, ConnectedEntity.ConnectedEntityType.Individual);
        var additionalEntities = await organisationRepository.GetConnectedEntities(sharedConsent.OrganisationId, ConnectedEntity.ConnectedEntityType.Organisation);

        var mappedAssociatedPersones = associatedPersons.Select(x => new AssociatedPerson()
        {
            Id = x.Guid,
            Name = x.RegisterName ?? string.Empty,
            Relationship = x.EntityType.ToString(),
            Uri = new Uri(string.Empty),
            Roles = new List<PartyRole>()
        }).ToList();

        var mappedAdditionalEntities = associatedPersons.Select(x => new OrganisationReference()
        {
            Id = x.Guid,
            Name = x.RegisterName ?? string.Empty,
            Roles = new List<PartyRole>(),
            Uri = new Uri(string.Empty)
        }).ToList();

        result.AssociatedPersons = mappedAssociatedPersones;
        result.AdditionalEntities = mappedAdditionalEntities;

        return result;
    }
}