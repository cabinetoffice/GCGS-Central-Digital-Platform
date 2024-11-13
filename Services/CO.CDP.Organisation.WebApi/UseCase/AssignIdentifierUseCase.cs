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

    public static IDictionary<string, string> IdentifierSchemesUK = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "GB-COH", "CompaniesHouse" },
        { "GB-CHC", "CharityCommissionEnglandWales" },
        { "GB-SC", "ScottishCharityRegulator" },
        { "GB-NIC", "CharityCommissionNorthernIreland" },
        { "GB-MPR", "MutualsPublicRegister" },
        { "GG-RCE", "GuernseyRegistry" },
        { "JE-FSC", "JerseyFinancialServicesCommission" },
        { "IM-CR", "IsleOfManCompaniesRegistry" },
        { "GB-NHS", "NHSOrganisationsRegistry" },
        { "GB-UKPRN", "UKRegisterOfLearningProviders" },
        { "VAT", "VAT" },
        { "Other", "Other" },
        { "GB-PPON", "Ppon" }
    };

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

    private static void ResetPrimaryIdentifiers(IEnumerable<OrganisationInformation.Persistence.Organisation.Identifier> identifiers)
    {
        foreach (var identifier in identifiers.Where(id => id.Primary))
        {
            identifier.Primary = false;
        }
    }

    public static bool IsPrimaryIdentifier(OrganisationInformation.Persistence.Organisation organisation, string newIdentifierSchemeName)
    {
        if (newIdentifierSchemeName == IdentifierSchemes.Vat || !IdentifierSchemesUK.ContainsKey(newIdentifierSchemeName))
        {
            return false;
        }

        if (organisation.Identifiers.Count == 0)
        {
            return true;
        }

        var primaryIdentifiers = organisation.Identifiers
           .Where(i => i.Primary && (i.Scheme == IdentifierSchemes.Other || i.Scheme == IdentifierSchemes.Ppon || !IdentifierSchemesUK.ContainsKey(i.Scheme)))
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