using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class RemoveOrganisationPartyUseCase(
    IOrganisationRepository orgRepo,
    IOrganisationPartiesRepository orgPartiesRepo
    ) : IUseCase<(Guid, RemoveOrganisationParty), bool>
{
    public async Task<bool> Execute((Guid, RemoveOrganisationParty) command)
    {
        (Guid organisationId, RemoveOrganisationParty removeParty) = command;

        var parentOrganisation = await orgRepo.Find(organisationId)
            ?? throw new UnknownOrganisationException($"Unknown parent organisation {organisationId}.");

        var childOrganisation = await orgRepo.Find(removeParty.OrganisationPartyId)
            ?? throw new UnknownOrganisationException($"Unknown child organisation {removeParty.OrganisationPartyId}.");

        var consortium = await orgPartiesRepo.Find(organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation party {organisationId}.");

        var childParty = consortium.Where(p => p.ChildOrganisationId == childOrganisation.Id).FirstOrDefault()
            ?? throw new UnknownOrganisationException($"Unknown organisation child party {organisationId}.");

        await orgPartiesRepo.Remove(childParty);

        return true;
    }
}