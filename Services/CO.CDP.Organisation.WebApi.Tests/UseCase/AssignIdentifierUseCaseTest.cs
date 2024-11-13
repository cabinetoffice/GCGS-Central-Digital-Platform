using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using FluentAssertions;
using Moq;
using static CO.CDP.Organisation.WebApi.UseCase.AssignIdentifierUseCase.AssignIdentifierException;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class AssignIdentifierUseCaseTest
{
    private readonly Mock<Persistence.IOrganisationRepository> _organisations = new();
    private readonly Mock<IIdentifierService> _identifierService = new();
    private AssignIdentifierUseCase UseCase => new(_organisations.Object, _identifierService.Object);

    [Fact]
    public async Task ItThrowsAnExceptionIfOrganisationIsNotFound()
    {
        var unknownOrganisationId = Guid.NewGuid();

        await UseCase.Invoking(u => u.Execute(new AssignOrganisationIdentifier
        {
            OrganisationId = unknownOrganisationId,
            Identifier = new OrganisationIdentifier
            {
                Id = "c0777aeb968b4113a27d94e55b10c1b4",
                Scheme = "GB-PPON",
                LegalName = "Acme Ltd"
            }
        })).Should().ThrowAsync<OrganisationNotFoundException>();
    }

    [Fact]
    public async Task ItAssignsPrimaryIdentifierIfOrganisationHasNoIdentifiersAssigned()
    {
        var organisation = GivenOrganisationExist(
            organisationId: Guid.NewGuid(),
            identifiers: []);

        var result = await UseCase.Execute(new AssignOrganisationIdentifier
        {
            OrganisationId = organisation.Guid,
            Identifier = new OrganisationIdentifier
            {
                Id = "c0777aeb968b4113a27d94e55b10c1b4",
                Scheme = "GB-PPON",
                LegalName = "Acme Ltd"
            }
        });

        _organisations.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
            o.Guid == organisation.Guid && o.Identifiers.Contains(new Persistence.Organisation.Identifier
            {
                Primary = true,
                Scheme = "GB-PPON",
                IdentifierId = "c0777aeb968b4113a27d94e55b10c1b4",
                LegalName = "Acme Ltd"
            }
            ))));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ItAssignsPponIdentifierAsPrimaryWhenOtherIdentifierExistsAsPrimary()
    {
        var organisation = GivenOrganisationExist(
            organisationId: Guid.NewGuid(),
            identifiers: [  new Persistence.Organisation.Identifier {
                LegalName = "Acme Ltd",
                Primary = true,
                Scheme = "Other"
            }
         ]);

        var result = await UseCase.Execute(new AssignOrganisationIdentifier
        {
            OrganisationId = organisation.Guid,
            Identifier = new OrganisationIdentifier
            {
                Id = "c0777aeb968b4113a27d94e55b10c1b4",
                Scheme = "GB-PPON",
                LegalName = "Acme Ltd"
            }
        });

        _organisations.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
         o.Guid == organisation.Guid && o.Identifiers.Contains(new Persistence.Organisation.Identifier
             {
                 Primary = true,
                 Scheme = "GB-PPON",
                 IdentifierId = "c0777aeb968b4113a27d94e55b10c1b4",
                 LegalName = "Acme Ltd"
             }) && o.Identifiers.Contains(new Persistence.Organisation.Identifier
             {
                 LegalName = "Acme Ltd",
                 Primary = false,
                 Scheme = "Other"
             })
         )));

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ItAssignsCompaniesHouseIdentifierAsPrimaryWhenPponIdentifierExistsAsPrimary()
    {
        var organisation = GivenOrganisationExist(
            organisationId: Guid.NewGuid(),
            identifiers: [  new Persistence.Organisation.Identifier {
                IdentifierId = "c0777aeb968b4113a27d94e55b10c1b4",
                Scheme = AssignIdentifierUseCase.IdentifierSchemes.Ppon,
                Primary = true,
                LegalName = "Acme Ltd"
            }
         ]);

        var result = await UseCase.Execute(new AssignOrganisationIdentifier
        {
            OrganisationId = organisation.Guid,
            Identifier = new OrganisationIdentifier
            {
                Id = "0123456789",
                Scheme = AssignIdentifierUseCase.IdentifierSchemes.CompaniesHouse,
                LegalName = "Acme Ltd"
            }
        });

        _organisations.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
         o.Guid == organisation.Guid && o.Identifiers.Contains(new Persistence.Organisation.Identifier
         {
             Primary = false,
             Scheme = AssignIdentifierUseCase.IdentifierSchemes.Ppon,
             IdentifierId = "c0777aeb968b4113a27d94e55b10c1b4",
             LegalName = "Acme Ltd"
         }) && o.Identifiers.Contains(new Persistence.Organisation.Identifier
         {
             IdentifierId = "0123456789",
             Scheme = AssignIdentifierUseCase.IdentifierSchemes.CompaniesHouse,
             LegalName = "Acme Ltd",
             Primary = true
         })
         )));

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ItAssignsOtherIdentifierAsPrimaryIfThereAreNoIdentifiers()
    {
        var organisation = GivenOrganisationExist(
            organisationId: Guid.NewGuid(),
            identifiers: [ ]);

        var result = await UseCase.Execute(new AssignOrganisationIdentifier
        {
            OrganisationId = organisation.Guid,
            Identifier = new OrganisationIdentifier
            {
                Scheme = "Other",
                LegalName = "Acme Ltd"
            }
        });

        _organisations.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
         o.Guid == organisation.Guid && o.Identifiers.Contains(new Persistence.Organisation.Identifier
         {
             Primary = true,
             Scheme = "Other",
             LegalName = "Acme Ltd"
         }))));

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ItAssignsNonPrimaryIdentifierIfTheOrganisationAlreadyHasIdentifiersAssigned()
    {
        var organisation = GivenOrganisationExist(
            organisationId: Guid.NewGuid(),
            identifiers:
            [
                new Persistence.Organisation.Identifier
                {
                    Primary = true,
                    Scheme = "GB-COH",
                    IdentifierId = "944432342",
                    LegalName = "Acme Ltd"
                }
            ]);

        var result = await UseCase.Execute(new AssignOrganisationIdentifier
        {
            OrganisationId = organisation.Guid,
            Identifier = new OrganisationIdentifier
            {
                Id = "c0777aeb968b4113a27d94e55b10c1b4",
                Scheme = "GB-PPON",
                LegalName = "Acme Ltd"
            }
        });

        _organisations.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
            o.Guid == organisation.Guid && o.Identifiers.Contains(new Persistence.Organisation.Identifier
            {
                Primary = false,
                Scheme = "GB-PPON",
                IdentifierId = "c0777aeb968b4113a27d94e55b10c1b4",
                LegalName = "Acme Ltd"
            }
            ))));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ItAssignsPPONPrimaryIdentifierIfOnlyInternationalIdentifierExists()
    {
        var organisation = GivenOrganisationExist(
            organisationId: Guid.NewGuid(),
            identifiers:
            [
                new Persistence.Organisation.Identifier
                {
                    Primary = true,
                    Scheme = "FR-COH",
                    IdentifierId = "944432342",
                    LegalName = "France Acme Ltd"
                }
            ]);

        var result = await UseCase.Execute(new AssignOrganisationIdentifier
        {
            OrganisationId = organisation.Guid,
            Identifier = new OrganisationIdentifier
            {
                Id = "c0777aeb968b4113a27d94e55b10c1b4",
                Scheme = "GB-PPON",
                LegalName = "France Acme Ltd"
            }
        });

        _organisations.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
            o.Guid == organisation.Guid && o.Identifiers.Contains(new Persistence.Organisation.Identifier
            {
                Primary = true,
                Scheme = "GB-PPON",
                IdentifierId = "c0777aeb968b4113a27d94e55b10c1b4",
                LegalName = "France Acme Ltd"
            }
            ))));
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ItAssignsUKPrimaryIdentifierIfInternationalAndPPONAndUKIdentifiersExists()
    {
        var organisation = GivenOrganisationExist(
            organisationId: Guid.NewGuid(),
            identifiers:
            [
                new Persistence.Organisation.Identifier
                {
                    Primary = false,
                    Scheme = "FR-COH",
                    IdentifierId = "944432342",
                    LegalName = "France Acme Ltd"
                },
                new Persistence.Organisation.Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "12944432342",
                    LegalName = "France Acme Ltd"
                }
            ]);

        var result = await UseCase.Execute(new AssignOrganisationIdentifier
        {
            OrganisationId = organisation.Guid,
            Identifier = new OrganisationIdentifier
            {
                Id = "c0777aeb968b4113a27d94e66b10c1b5",
                Scheme = "GB-COH",
                LegalName = "France Acme Ltd"
            }
        });

        _organisations.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
            o.Guid == organisation.Guid && o.Identifiers.Contains(new Persistence.Organisation.Identifier
            {
                Primary = true,
                Scheme = "GB-COH",
                IdentifierId = "c0777aeb968b4113a27d94e66b10c1b5",
                LegalName = "France Acme Ltd"
            }
            ))));
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("Other", "12345678", "Acme Ltd", false)]    
    [InlineData("GB-COH", "944432342", "Acme Ltd", false)]
    public async Task ItDoesNotAssignsInternationalPrimaryIdentifier(string scheme, string identifierId, string legalName, bool primary)
    {
        var organisation = GivenOrganisationExist(
            organisationId: Guid.NewGuid(),
            identifiers:
            [
                new Persistence.Organisation.Identifier
                {
                    Primary = primary,
                    Scheme = scheme,
                    IdentifierId = identifierId,
                    LegalName = legalName
                },
                 new Persistence.Organisation.Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "8765456",
                    LegalName = legalName
                }
            ]);

        var result = await UseCase.Execute(new AssignOrganisationIdentifier
        {
            OrganisationId = organisation.Guid,
            Identifier = new OrganisationIdentifier
            {
                Id = "c0777aeb968b4113a27d94e55b16876788",
                Scheme = "FR-CPR",
                LegalName = "France CPR Ltd"
            }
        });

        _organisations.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
            o.Guid == organisation.Guid && o.Identifiers.Contains(new Persistence.Organisation.Identifier
            {
                Primary = false,
                Scheme = "FR-CPR",
                IdentifierId = "c0777aeb968b4113a27d94e55b16876788",
                LegalName = "France CPR Ltd"
            }
            ))));
        result.Should().BeTrue();
    }

    [Fact]  
    public void IsPrimaryIdentifier_ShouldReturnFalse_WhenSchemeIsNotInIdentifierSchemesUK()
    {
        var organisation = GivenOrganisationExist(
            organisationId: Guid.NewGuid(),
            identifiers:
            [
                new Persistence.Organisation.Identifier
                {
                    Primary = true,
                    Scheme = "FR-CDH",
                    IdentifierId = "678932342",
                    LegalName = "France Acme Ltd"
                }
            ]);
       
        var result = AssignIdentifierUseCase.IsPrimaryIdentifier(organisation, "FR-GHH");
        
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ItThrowsAnExceptionIfIdentifierIsAlreadyAssigned()
    {
        var organisation = GivenOrganisationExist(
            organisationId: Guid.NewGuid(),
            identifiers:
            [
                new Persistence.Organisation.Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "c0777aeb968b4113a27d94e55b10c1b4",
                    LegalName = "Acme Ltd"
                }
            ]);

        await UseCase.Invoking(u => u.Execute(new AssignOrganisationIdentifier
        {
            OrganisationId = organisation.Guid,
            Identifier = new OrganisationIdentifier
            {
                Id = "c0777aeb968b4113a27d94e55b10c1b4",
                Scheme = "GB-PPON",
                LegalName = "Acme Ltd"
            }
        })).Should().ThrowAsync<IdentifierAlreadyAssigned>();
    }

    private Persistence.Organisation GivenOrganisationExist(
        Guid organisationId,
        List<Persistence.Organisation.Identifier>? identifiers = null)
    {
        var organisation = new Persistence.Organisation
        {
            Name = "Acme Ltd",
            Guid = organisationId,
            Identifiers = identifiers ?? [],
            Addresses = [],
            SupplierInfo = new Persistence.Organisation.SupplierInformation(),
            Tenant = new Persistence.Tenant
            {
                Guid = Guid.Parse("68a9d5a1-4330-49c9-be64-7d5a8e8ae18d"),
                Name = "Tenant"
            }
        };

        _organisations.Setup(repo => repo.Find(organisationId)).ReturnsAsync(organisation);

        return organisation;
    }
}