using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class RevokeAuthenticationKeyUseCase(
    IOrganisationRepository organisationRepository,
    IAuthenticationKeyRepository keyRepository
   )
    : IUseCase<(Guid organisationId, RevokeAuthenticationKey revokeAuthentication), bool>
{
    public async Task<bool> Execute((Guid organisationId, RevokeAuthenticationKey revokeAuthentication) command)
    {
        var revokeAuthKey = command.revokeAuthentication;

        _ = await organisationRepository.Find(command.organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        if (string.IsNullOrEmpty(revokeAuthKey.Key))
            throw new EmptyAuthenticationKeyException($"Empty Key of Revoke AuthenticationKey for organisation {command.organisationId}.");

        if (string.IsNullOrEmpty(revokeAuthKey.Name))
            throw new EmptyAuthenticationKeyNameException($"Empty Name of Revoke AuthenticationKey for organisation {command.organisationId}.");

        var authorisationKey = await keyRepository.FindByKeyNameAndOrganisationId(revokeAuthKey.Key, revokeAuthKey.Name, command.organisationId)
            ?? throw new UnknownAuthenticationKeyException($"Unknown Authentication Key - name {revokeAuthKey.Name} for organisation {command.organisationId}.");

        authorisationKey.Revoked = true;

        await keyRepository.Save(authorisationKey);

        return await Task.FromResult(true);
    }
}