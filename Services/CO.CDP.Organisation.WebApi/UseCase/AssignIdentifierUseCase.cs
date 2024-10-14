using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class AssignIdentifierUseCase(IOrganisationRepository organisations, IIdentifierService identifierService)
    : IUseCase<AssignOrganisationIdentifier, bool>
{
    public class IdentifierSchemes
    {
        public const string Ppon = "GB-PPON";
        public const string Other = "Other";
        public const string Vat = "VAT";
        public const string CompaniesHouse = "GB-COH";
    }

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

    private OrganisationInformation.Persistence.Organisation AssignIdentifier(
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
            Primary = IsPrimaryIdentifier(organisation, command.Identifier.Scheme),
            Uri = identifierService.GetRegistryUri(command.Identifier.Scheme, command.Identifier.Id)
        });

        return organisation;
    }

    private static void ResetIdentifierPrimaryToFalse(OrganisationInformation.Persistence.Organisation.Identifier? identifier)
    {
        if (identifier != null)
        {
            identifier.Primary = false;
        }    
    }

    public static bool IsPrimaryIdentifier(OrganisationInformation.Persistence.Organisation organisation, string newIdentifierSchemeName)
    {
        if (newIdentifierSchemeName == IdentifierSchemes.Vat)
        {
            return false;
        }

        bool isPrimary = organisation.Identifiers.Count == 0;

        if (organisation.Identifiers.Any(i => i.Scheme == IdentifierSchemes.Other && i.Primary) ||
            organisation.Identifiers.Any(i => i.Scheme == IdentifierSchemes.Ppon && i.Primary))
        {
            ResetIdentifierPrimaryToFalse(organisation.Identifiers.FirstOrDefault(i => i.Scheme == IdentifierSchemes.Other && i.Primary));
            ResetIdentifierPrimaryToFalse(organisation.Identifiers.FirstOrDefault(i => i.Scheme == IdentifierSchemes.Ppon && i.Primary));

            isPrimary = true;
        }

        return isPrimary;
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