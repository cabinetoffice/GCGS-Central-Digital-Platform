using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class RevokeAuthenticationKeyUseCase(
    IOrganisationRepository organisationRepository,
    IAuthenticationKeyRepository keyRepository
   )
    : IUseCase<(Guid organisationId, string keyName), bool>
{
    public async Task<bool> Execute((Guid organisationId, string keyName) command)
    {
        var revokeAuthKeyName = command.keyName;

        _ = await organisationRepository.Find(command.organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        if (string.IsNullOrEmpty(revokeAuthKeyName))
            throw new EmptyAuthenticationKeyNameException($"Empty Name of Revoke AuthenticationKey for organisation {command.organisationId}.");

        var authorisationKey = await keyRepository.FindByKeyNameAndOrganisationId(revokeAuthKeyName, command.organisationId)
            ?? throw new UnknownAuthenticationKeyException($"Unknown Authentication Key - name {revokeAuthKeyName} for organisation {command.organisationId}.");

        authorisationKey.Revoked = true;
        authorisationKey.RevokedOn = DateTimeOffset.UtcNow;

        await keyRepository.Save(authorisationKey);

        return await Task.FromResult(true);
    }
}