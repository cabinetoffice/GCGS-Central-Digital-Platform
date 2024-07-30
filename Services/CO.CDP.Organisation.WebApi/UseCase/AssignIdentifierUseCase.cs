using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class AssignIdentifierUseCase(IOrganisationRepository organisations)
    : IUseCase<AssignOrganisationIdentifier, Boolean>
{
    public async Task<bool> Execute(AssignOrganisationIdentifier command)
    {
        await FindOrganisation(command)
            .AndThen(o => AssignIdentifier(command, o))
            .AndThen(o =>
            {
                organisations.Save(o);
                return o;
            });

        return true;
    }

    private static OrganisationInformation.Persistence.Organisation AssignIdentifier(
        AssignOrganisationIdentifier command, OrganisationInformation.Persistence.Organisation organisation)
    {
        if (organisation.Identifiers.Any(i =>
                i.Scheme == command.Identifier.Scheme && i.IdentifierId == command.Identifier.Id))
        {
            throw new AssignIdentifierException.IdentifierAlreadyAssigned(command.OrganisationId, command.Identifier);
        }

        organisation.Identifiers.Add(new OrganisationInformation.Persistence.Organisation.Identifier
        {
            IdentifierId = command.Identifier.Id,
            Scheme = command.Identifier.Scheme,
            LegalName = command.Identifier.LegalName,
            Primary = organisation.Identifiers.Count == 0
        });

        return organisation;
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

        public class IdentifierAlreadyAssigned(Guid organisationId, OrganisationIdentifier identifier)
            : AssignIdentifierException(
                $"Identifier `{identifier.Scheme}:{identifier.Id}` is already assigned to the organisation `{organisationId}`.");
    }
}