using CO.CDP.AwsServices.Sqs;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class AssignIdentifierUseCase(IOrganisationRepository organisations, IIdentifierService identifierService,
    ILogger<AssignIdentifierUseCase> logger)
    : IUseCase<AssignOrganisationIdentifier, bool>
{
    public class IdentifierSchemes
    {
        public const string Ppon = "GB-PPON";
        public const string Other = "Other";
        public const string Vat = "VAT";
        public const string CompaniesHouse = "GB-COH";
    }

    private static ISet<string> IdentifierSchemesUK = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "GB-COH",
        "GB-CHC",
        "GB-SC",
        "GB-NIC",
        "GB-MPR",
        "GG-RCE",
        "JE-FSC",
        "IM-CR",
        "GB-NHS",
        "GB-UKPRN",
        "VAT",
        "Other",
        "GB-PPON"
    };

    public async Task<bool> Execute(AssignOrganisationIdentifier command)
    {
        logger.LogInformation("Assigning identifier for OrganisationId: {OrganisationId} IdentifierId: {IdentifierId}",
            command.OrganisationId,
            command.Identifier.Id);

        try
        {
            await FindOrganisation(command)
                .AndThen(o => AssignIdentifier(command, o))
                .AndThen(o =>
                {
                    organisations.Save(o);
                    return o;
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to assign identifier for OrganisationId: {OrganisationId} IdentifierId: {IdentifierId}",
                command.OrganisationId,
                command.Identifier.Id);
            throw;
        }

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
            Uri = identifierService.GetRegistryUri(command.Identifier.Scheme, command.Identifier.Id, organisation.Guid)
        });

        return organisation;
    }

    private static void ResetPrimaryIdentifiers(IEnumerable<OrganisationInformation.Persistence.Organisation.Identifier> identifiers)
    {
        foreach (var identifier in identifiers.Where(id => id.Primary))
        {
            identifier.Primary = false;
        }
    }

    public static bool IsPrimaryIdentifier(OrganisationInformation.Persistence.Organisation organisation, string newIdentifierSchemeName)
    {
        if (newIdentifierSchemeName == IdentifierSchemes.Vat || !IdentifierSchemesUK.Contains(newIdentifierSchemeName))
        {
            return false;
        }

        if (organisation.Identifiers.Count == 0)
        {
            return true;
        }

        var primaryIdentifiers = organisation.Identifiers
           .Where(i => i.Primary && (i.Scheme == IdentifierSchemes.Other || i.Scheme == IdentifierSchemes.Ppon || !IdentifierSchemesUK.Contains(i.Scheme)))
           .ToList();

        if (primaryIdentifiers.Any())
        {
            ResetPrimaryIdentifiers(primaryIdentifiers);
            return true;
        }

        return false;
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