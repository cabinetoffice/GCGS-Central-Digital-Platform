using FluentAssertions;
using static CO.CDP.OrganisationInformation.Persistence.Tests.EntityFactory;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabasePersonTenantLookupTest(OrganisationInformationPostgreSqlFixture postgreSql)
    : IClassFixture<OrganisationInformationPostgreSqlFixture>
{
    [Fact]
    public async Task ItFindsThePersonWithAllTheirTenants()
    {
        using var personRepository = PersonRepository();
        using var tenantRepository = TenantRepository();

        var acmeCoPersonUrn = "urn:fdc:gov.uk:2022:7wTqYGMFQxgukTSpSI2G0dMwe9";
        var widgetCoPersonUrn = "urn:fdc:gov.uk:2022:6fTvD1cMhQNJxrLZSyBgo5";
        var acmeCoWithNoTenantPersonUrn = "urn:fdc:gov.uk:2022:2jGsPVWXzmR0Nd8qBEaUlY";
        var acmeCoWithNoOrganisationPersonUrn = "urn:fdc:gov.uk:2022:Mwe9xZbHnRcFJLtYv3Kp7Q";

        var adminPersonScopes = new List<string> { "ADMIN" };
        var userPersonScopes = new List<string> { "USER" };
        var personUserScopes = new List<string> { "SUPPORTADMIN" };

        var acmeCoTenant = GivenTenant(name: "Acme Co Tenant");
        var acmeCoOrganisation = GivenOrganisation(
            name: "Acme Co Organisation",
            tenant: acmeCoTenant);
        var acmeCoPerson = GivenPerson(
            userUrn: acmeCoPersonUrn,
            scopes: personUserScopes,
            tenant: acmeCoTenant,
            organisationsWithScope: [(acmeCoOrganisation, adminPersonScopes)]
        );
        var acmeCoPersonWithNoTenantConnection = GivenPerson(
            userUrn: acmeCoWithNoTenantPersonUrn,
            scopes: personUserScopes,
            organisationsWithScope: [(acmeCoOrganisation, ["USER"])]
        );
        var acmeCoPersonWithNoOrganisationConnection = GivenPerson(
            userUrn: acmeCoWithNoOrganisationPersonUrn,
            scopes: personUserScopes,
            tenant: acmeCoTenant
        );

        var widgetCoTenant = GivenTenant(name: "Widget Co Org");
        var widgetCoOrganisation = GivenOrganisation(
            name: "Widget Co Organisation",
            tenant: widgetCoTenant);
        var widgetCoPerson = GivenPerson(
            userUrn: widgetCoPersonUrn,
            scopes: personUserScopes,
            tenant: widgetCoTenant,
            organisationsWithScope: [(widgetCoOrganisation, ["USER"]), (acmeCoOrganisation, ["USER"])]
        );

        personRepository.Save(acmeCoPerson);
        personRepository.Save(widgetCoPerson);
        personRepository.Save(acmeCoPersonWithNoTenantConnection);
        personRepository.Save(acmeCoPersonWithNoOrganisationConnection);

        var acmeCoPersonLookup = await tenantRepository.LookupTenant(acmeCoPersonUrn);
        acmeCoPersonLookup.Should().BeEquivalentTo(new TenantLookup
        {
            User = new TenantLookup.PersonUser
            {
                Email = acmeCoPerson.Email,
                Name = $"{acmeCoPerson.FirstName} {acmeCoPerson.LastName}",
                Urn = acmeCoPersonUrn,
                Scopes = personUserScopes
            },
            Tenants =
            [
                new()
                {
                    Id = acmeCoTenant.Guid,
                    Name = acmeCoTenant.Name,
                    Organisations =
                    [
                        new TenantLookup.Organisation
                        {
                            Id = acmeCoOrganisation.Guid,
                            Name = acmeCoOrganisation.Name,
                            Roles = acmeCoOrganisation.Roles,
                            PendingRoles = acmeCoOrganisation.PendingRoles,
                            Scopes = adminPersonScopes
                        }
                    ]
                }
            ]
        });


        var widgetCoPersonLookup = await tenantRepository.LookupTenant(widgetCoPersonUrn);
        widgetCoPersonLookup.Should().BeEquivalentTo(new TenantLookup
        {
            User = new TenantLookup.PersonUser
            {
                Email = widgetCoPerson.Email,
                Name = $"{widgetCoPerson.FirstName} {widgetCoPerson.LastName}",
                Urn = widgetCoPersonUrn,
                Scopes = personUserScopes
            },
            Tenants =
            [
                new()
                {
                    Id = widgetCoTenant.Guid,
                    Name = widgetCoTenant.Name,
                    Organisations =
                    [
                        new TenantLookup.Organisation
                        {
                            Id = widgetCoOrganisation.Guid,
                            Name = widgetCoOrganisation.Name,
                            Roles = widgetCoOrganisation.Roles,
                            PendingRoles = widgetCoOrganisation.PendingRoles,
                            Scopes = userPersonScopes
                        }
                    ]
                }
            ]
        });


        var acmeCoPersonWithNoTenantConnectionLookup = await tenantRepository.LookupTenant(acmeCoWithNoTenantPersonUrn);
        acmeCoPersonWithNoTenantConnectionLookup.Should().BeEquivalentTo(new TenantLookup
        {
            User = new TenantLookup.PersonUser
            {
                Email = acmeCoPersonWithNoTenantConnection.Email,
                Name = $"{acmeCoPersonWithNoTenantConnection.FirstName} {acmeCoPersonWithNoTenantConnection.LastName}",
                Urn = acmeCoWithNoTenantPersonUrn,
                Scopes = personUserScopes
            },
            Tenants = new List<TenantLookup.Tenant>()
        });


        var acmeCoPersonWithNoOrganisationConnectionLookup = await tenantRepository.LookupTenant(acmeCoWithNoOrganisationPersonUrn);
        acmeCoPersonWithNoOrganisationConnectionLookup.Should().BeEquivalentTo(new TenantLookup
        {
            User = new TenantLookup.PersonUser
            {
                Email = acmeCoPersonWithNoOrganisationConnection.Email,
                Name = $"{acmeCoPersonWithNoOrganisationConnection.FirstName} {acmeCoPersonWithNoOrganisationConnection.LastName}",
                Urn = acmeCoWithNoOrganisationPersonUrn,
                Scopes = personUserScopes
            },
            Tenants =
            [
                new()
                {
                    Id = acmeCoTenant.Guid,
                    Name = acmeCoTenant.Name,
                    Organisations =
                    [
                        new TenantLookup.Organisation
                        {
                            Id = acmeCoOrganisation.Guid,
                            Name = acmeCoOrganisation.Name,
                            Roles = acmeCoOrganisation.Roles,
                            PendingRoles = acmeCoOrganisation.PendingRoles,
                            Scopes = []
                        }
                    ]
                }
            ]
        });

    }

    private DatabaseTenantRepository TenantRepository()
        => new(GetDbContext());

    private DatabasePersonRepository PersonRepository()
        => new(GetDbContext());

    private OrganisationInformationContext? context = null;

    private OrganisationInformationContext GetDbContext()
    {
        context = context ?? postgreSql.OrganisationInformationContext();

        return context;
    }
}