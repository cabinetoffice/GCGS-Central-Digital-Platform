using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class AddOrganisationPartyUseCase(
    IOrganisationRepository orgRepo,
    IShareCodeRepository shareCodeRepo,
    IOrganisationPartiesRepository orgPartiesRepo
    ) : IUseCase<(Guid, AddOrganisationParty), bool>
{
    public async Task<bool> Execute((Guid, AddOrganisationParty) command)
    {
        (Guid organisationId, AddOrganisationParty addParty) = command;

        var parentOrganisation = await orgRepo.Find(organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {organisationId}.");

        var childOrganisation = await orgRepo.Find(addParty.OrganisationPartyId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {addParty.OrganisationPartyId}.");

        int? sharedConsentId = null;

        if (!string.IsNullOrWhiteSpace(addParty.ShareCode))
        {
            var sharedConsents = await shareCodeRepo.GetShareCodesAsync(addParty.OrganisationPartyId);

            var sharedConsent = sharedConsents.FirstOrDefault(s =>
                    string.Equals(s.ShareCode, addParty.ShareCode, StringComparison.InvariantCultureIgnoreCase));

            if (sharedConsent == null
                || sharedConsent.SubmissionState != OrganisationInformation.Persistence.Forms.SubmissionState.Submitted
                || sharedConsent.OrganisationId != childOrganisation.Id)
            {
                throw new OrganisationShareCodeInvalid(addParty.ShareCode);
            }

            sharedConsentId = sharedConsent.Id;
        }

        await orgPartiesRepo.Save(
            new OrganisationInformation.Persistence.OrganisationParty
            {
                ParentOrganisationId = parentOrganisation.Id,
                ChildOrganisationId = childOrganisation.Id,
                OrganisationRelationship = (OrganisationInformation.Persistence.OrganisationRelationship)addParty.OrganisationRelationship,
                SharedConsentId = sharedConsentId,
            });

        return true;
    }
}