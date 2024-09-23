using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
namespace CO.CDP.Organisation.WebApi.UseCase;

public class RegisterAuthenticationKeyUseCase(
    IAuthenticationKeyRepository keyRepository,
    IOrganisationRepository organisationRepository)
    : IUseCase<(Guid organisationId, RegisterAuthenticationKey authKey), bool>
{
    public async Task<bool> Execute((Guid organisationId, RegisterAuthenticationKey authKey) command)
    {
        var authenticationKey = command.authKey;

        var organisation = await organisationRepository.Find(command.organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        await keyRepository.Save(new OrganisationInformation.Persistence.AuthenticationKey
        {
            Key = authenticationKey.Key,
            Name = authenticationKey.Name,
            OrganisationId = organisation.Id,
        });

        return await Task.FromResult(true);
    }
}