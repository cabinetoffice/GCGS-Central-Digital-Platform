using CO.CDP.MQ;
using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdatePersonToOrganisationUseCase(
    IOrganisationRepository organisationRepository,
    IPublisher publisher,
    ILogger<UpdatePersonToOrganisationUseCase> logger)
    : IUseCase<(Guid organisationId, Guid personId, UpdatePersonToOrganisation updatePerson), bool>
{
    public async Task<bool> Execute(
        (Guid organisationId, Guid personId, UpdatePersonToOrganisation updatePerson) command)
    {
        if (!command.updatePerson.Scopes.Any())
            throw new EmptyPersonRoleException($"Empty Scope of Person {command.personId}.");

        var organisationPerson = await organisationRepository.FindOrganisationPerson(
                                     command.organisationId, command.personId)
                                 ?? throw new UnknownOrganisationException(
                                     $"Unknown organisation {command.organisationId} or Person {command.personId}.");

        organisationPerson.Scopes = command.updatePerson.Scopes;
        organisationRepository.TrackOrganisationPerson(organisationPerson);

        logger.LogInformation("Publishing PersonScopesUpdated for org {OrgId}, person {PersonId}, scopes=[{Scopes}]",
            command.organisationId, command.personId, string.Join(",", command.updatePerson.Scopes));

        await publisher.Publish(new PersonScopesUpdated
        {
            OrganisationId = command.organisationId.ToString(),
            PersonId = command.personId.ToString(),
            Scopes = command.updatePerson.Scopes
        });

        return true;
    }
}