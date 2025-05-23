using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdateOrganisationPartyUseCase(
    IOrganisationRepository orgRepo,
    IShareCodeRepository shareCodeRepo,
    IOrganisationPartiesRepository orgPartiesRepo
    ) : IUseCase<(Guid, UpdateOrganisationParty), bool>
{
    public async Task<bool> Execute((Guid, UpdateOrganisationParty) command)
    {
        (Guid organisationId, UpdateOrganisationParty updateParty) = command;

        var parentOrganisation = await orgRepo.Find(organisationId)
            ?? throw new UnknownOrganisationException($"Unknown parent organisation {organisationId}.");

        var childOrganisation = await orgRepo.Find(updateParty.OrganisationPartyId)
            ?? throw new UnknownOrganisationException($"Unknown child organisation {updateParty.OrganisationPartyId}.");

        int? sharedConsentId = null;

        if (!string.IsNullOrWhiteSpace(updateParty.ShareCode))
        {
            var sharedConsents = await shareCodeRepo.GetShareCodesAsync(updateParty.OrganisationPartyId);
            var sharedConsent = sharedConsents.FirstOrDefault(s =>
                    string.Equals(s.ShareCode, updateParty.ShareCode, StringComparison.InvariantCultureIgnoreCase));

            if (sharedConsent == null
                || sharedConsent.SubmissionState != OrganisationInformation.Persistence.Forms.SubmissionState.Submitted
                || sharedConsent.OrganisationId != childOrganisation.Id)
            {
                throw new OrganisationShareCodeInvalid(updateParty.ShareCode);
            }

            sharedConsentId = sharedConsent.Id;
        }

        var consortium = await orgPartiesRepo.Find(organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation party {organisationId}.");

        var childParty = consortium.Where(p => p.ChildOrganisationId == childOrganisation.Id).FirstOrDefault()
            ?? throw new UnknownOrganisationException($"Unknown organisation child party {organisationId}.");

        childParty.SharedConsentId = sharedConsentId;

        await orgPartiesRepo.Save(childParty);

        return true;
    }
}