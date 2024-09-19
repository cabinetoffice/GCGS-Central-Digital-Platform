using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using FluentAssertions;
using Moq;
using Organisation = CO.CDP.OrganisationInformation.Persistence.Organisation;
using Person = CO.CDP.OrganisationInformation.Persistence.Person;

public class GetApprovableOrganisationsUseCaseTest
{
    private readonly Mock<IOrganisationRepository> _organisationRepositoryMock;
    private readonly GetApprovableOrganisationsUseCase _useCase;

    public GetApprovableOrganisationsUseCaseTest()
    {
        _organisationRepositoryMock = new Mock<IOrganisationRepository>();
        _useCase = new GetApprovableOrganisationsUseCase(_organisationRepositoryMock.Object);
    }

    [Fact]
    public async Task Execute_NoOrganisations_ReturnsEmptyList()
    {
        var organisations = new List<Organisation>();

        var command = ("buyers", 10, 0);
        _organisationRepositoryMock.Setup(repo => repo.Get("buyers"))
            .ReturnsAsync(organisations);

        var result = await _useCase.Execute(command);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_OrganisationsExist_ReturnsApprovableOrganisations()
    {
        var command = ("suppliers", 10, 0);
        var organisations = new List<Organisation>
        {
            new Organisation
            {
                Guid = Guid.NewGuid(),
                Name = "Org 1",
                Identifiers = new List<Organisation.Identifier>
                {
                    new Organisation.Identifier
                    {
                        Scheme = "TestScheme",
                        IdentifierId = "123",
                        LegalName = "Legal Org 1",
                        Primary = false
                    }
                },
                ContactPoints = new List<Organisation.ContactPoint>
                {
                    new Organisation.ContactPoint
                    {
                        Email = "contact@org1.com"
                    }
                },
                ApprovedOn = DateTime.UtcNow,
                ApprovedBy = new Person
                {
                    Guid = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe",
                    Email = null
                },
                Tenant = null
            },
            new Organisation
            {
                Guid = Guid.NewGuid(),
                Name = "Org 2",
                SupplierInfo = new Organisation.SupplierInformation(), // To test GetRole method returning "supplier"
                Identifiers = new List<Organisation.Identifier>
                {
                    new Organisation.Identifier
                    {
                        Scheme = "CDP-PPON",
                        IdentifierId = "456",
                        LegalName = null,
                        Primary = false
                    }
                },
                ContactPoints = new List<Organisation.ContactPoint>(),
                ApprovedOn = null,
                ApprovedBy = null,
                Tenant = null
            }
        };

        _organisationRepositoryMock.Setup(repo => repo.Get("suppliers"))
            .ReturnsAsync(organisations);

        var result = await _useCase.Execute(command);

        result.Should().HaveCount(2);

        var approvableOrganisations = result.ToList();

        approvableOrganisations[0].Should().BeEquivalentTo(new ApprovableOrganisation
        {
            Id = organisations[0].Guid,
            Name = "Org 1",
            Identifiers = new List<Identifier>
            {
                new Identifier { Scheme = "TestScheme", Id = "123", LegalName = "Legal Org 1" }
            },
            Role = "other",
            Email = "contact@org1.com",
            ApprovedOn = organisations[0].ApprovedOn,
            ApprovedById = organisations[0].ApprovedBy.Guid,
            ApprovedByName = "John Doe"
        }, options => options.Excluding(o => o.Ppon)); // Ppon is not set for this org

        approvableOrganisations[1].Should().BeEquivalentTo(new ApprovableOrganisation
        {
            Id = organisations[1].Guid,
            Name = "Org 2",
            Role = "supplier",
            Ppon = "456",
            Identifiers = null,
            Email = null
        }, options => options.Excluding(o => o.Identifiers).Excluding(o => o.Email).Excluding(o => o.ApprovedOn).Excluding(o => o.ApprovedById).Excluding(o => o.ApprovedByName));

        _organisationRepositoryMock.Verify(repo => repo.Get("suppliers"), Times.Once);
    }

    [Fact]
    public async Task Execute_OrganisationWithBuyerInfo_ReturnsRoleBuyer()
    {
        var command = ("buyers", 10, 0);
        var organisation = new Organisation
        {
            Guid = Guid.NewGuid(),
            Name = "Buyer Org",
            BuyerInfo = new Organisation.BuyerInformation(),
            Identifiers = new List<Organisation.Identifier>(),
            Tenant = null
        };

        _organisationRepositoryMock.Setup(repo => repo.Get("buyers"))
            .ReturnsAsync(new List<Organisation> { organisation });

        var result = await _useCase.Execute(command);

        result.Should().ContainSingle();
        result.First().Role.Should().Be("buyer");

        _organisationRepositoryMock.Verify(repo => repo.Get("buyers"), Times.Once);
    }
}
