using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class RemovePersonInviteFromOrganisationUseCase(IPersonInviteRepository personInviteRepository) : IUseCase<(Guid OrganisationId, Guid PersonInviteId), bool>
{
    public async Task<bool> Execute((Guid OrganisationId, Guid PersonInviteId) command)
    {
        var personInvites = await personInviteRepository.FindByOrganisation(command.OrganisationId);

        var personInvite = personInvites.FirstOrDefault(p => p.Guid == command.PersonInviteId);

        if (personInvite == null) return await Task.FromResult(false);

        bool isDeleteSuccess = await personInviteRepository.Delete(personInvite);

        return await Task.FromResult(isDeleteSuccess);
    }
}