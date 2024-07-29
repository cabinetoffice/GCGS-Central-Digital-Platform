using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class AssignIdentifierUseCase(IOrganisationRepository organisations)
    : IUseCase<AssignOrganisationIdentifier, Boolean>
{
    public async Task<bool> Execute(AssignOrganisationIdentifier command)
    {
        var organisation = await FindOrganisation(command);

        organisation.Identifiers.Add(new OrganisationInformation.Persistence.Organisation.Identifier
        {
            IdentifierId = command.Identifier.Id,
            Scheme = command.Identifier.Scheme,
            LegalName = command.Identifier.LegalName,
            Primary = organisation.Identifiers.Count == 0
        });
        organisations.Save(organisation);

        return true;
    }

    private async Task<OrganisationInformation.Persistence.Organisation> FindOrganisation(
        AssignOrganisationIdentifier command)
    {
        var organisation = await organisations.Find(command.OrganisationId);
        if (organisation is null)
        {
            throw new AssignIdentifierException.OrganisationNotFoundException(command.OrganisationId);
        }

        return organisation;
    }

    public class AssignIdentifierException(string message, Exception? cause = null) : Exception(message, cause)
    {
        public class OrganisationNotFoundException(Guid organisationId)
            : AssignIdentifierException($"Organisation not found: `{organisationId}`.");
    }
}