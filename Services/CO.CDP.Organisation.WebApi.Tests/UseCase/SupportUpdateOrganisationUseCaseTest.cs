using FluentAssertions;
using Moq;
using CO.CDP.Organisation.WebApi.Model;
using Persistence = CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.GovUKNotify;
using Microsoft.Extensions.Configuration;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.Extensions.Logging;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class SupportUpdateOrganisationUseCaseTests
{
    private readonly Mock<IOrganisationRepository> _mockOrganisationRepository;
    private readonly Mock<IPersonRepository> _mockPersonRepository;
    private readonly Mock<IGovUKNotifyApiClient> _notifyApiClient = new();
    private readonly Mock<ILogger<SupportUpdateOrganisationUseCase>> _logger = new();
    private readonly SupportUpdateOrganisationUseCase _useCase;
    private readonly Persistence.Organisation _organisation;
    private readonly Persistence.Person _person;

    public SupportUpdateOrganisationUseCaseTests()
    {
        var inMemorySettings = new List<KeyValuePair<string, string?>>
        {
            new("GOVUKNotify:PersonInviteEmailTemplateId", "test-template-id"),
            new("OrganisationAppUrl", "http://baseurl/"),
            new("GOVUKNotify:RequestReviewApplicationEmailTemplateId", "template-id"),
            new("GOVUKNotify:SupportAdminEmailAddress", "admin@example.com"),
            new("GOVUKNotify:BuyerApprovedEmailTemplateId", "buyer-approval-template-id"),
            new("GOVUKNotify:BuyerRejectedEmailTemplateId", "buyer-rejection-template-id"),
        };

        IConfiguration mockConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _mockOrganisationRepository = new Mock<IOrganisationRepository>();
        _mockPersonRepository = new Mock<IPersonRepository>();
        _useCase = new SupportUpdateOrganisationUseCase(
            _mockOrganisationRepository.Object,
            _mockPersonRepository.Object,
            _notifyApiClient.Object,
            mockConfiguration,
            _logger.Object);
        _organisation = new Persistence.Organisation
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Roles = [PartyRole.Tenderer],
            PendingRoles = [PartyRole.Buyer],
            Tenant = null!,
            Name = null!,
            Type = OrganisationType.Organisation,
            ContactPoints = [new Persistence.ContactPoint { Email = "org-email@test.com" }]
        };

        _person = new Persistence.Person
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Johnson",
            Email = "john@johnson.com",
            UserUrn = "urn:1234"
        };
    }

    [Fact]
    public async Task Execute_WhenOrganisationIsUnknown_ShouldThrowUnknownOrganisationException()
    {
        var organisationId = Guid.NewGuid();
        var supportUpdateOrganisation = new SupportUpdateOrganisation
        {
            Organisation = new SupportOrganisationInfo
            {
                ReviewedById = Guid.NewGuid(),
                Approved = true
            },
            Type = SupportOrganisationUpdateType.Review
        };

        _mockOrganisationRepository.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(null as OrganisationInformation.Persistence.Organisation);

        Func<Task> action = async () => await _useCase.Execute((organisationId, supportUpdateOrganisation));

        await action.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {organisationId}.");
    }

    [Fact]
    public async Task Execute_WhenPersonIsUnknown_ShouldThrowUnknownPersonException()
    {
        var personId = Guid.NewGuid();
        var supportUpdateOrganisation = new SupportUpdateOrganisation
        {
            Organisation = new SupportOrganisationInfo
            {
                ReviewedById = personId,
                Approved = true
            },
            Type = SupportOrganisationUpdateType.Review
        };

        _mockOrganisationRepository.Setup(repo => repo.Find(_organisation.Guid))
            .ReturnsAsync(_organisation);

        _mockPersonRepository.Setup(repo => repo.Find(personId))
            .ReturnsAsync(null as Persistence.Person);

        Func<Task> action = async () => await _useCase.Execute((_organisation.Guid, supportUpdateOrganisation));

        await action.Should().ThrowAsync<UnknownPersonException>()
            .WithMessage($"Unknown person {personId}.");
    }

    [Fact]
    public async Task Execute_WhenUpdateIsReviewAndApprovedAndEmailIsSent_ShouldUpdateOrganisationReviewDetails()
    {
        var supportUpdateOrganisation = new SupportUpdateOrganisation
        {
            Organisation = new SupportOrganisationInfo
            {
                ReviewedById = _person.Guid,
                Approved = true
            },
            Type = SupportOrganisationUpdateType.Review
        };

        _mockOrganisationRepository.Setup(repo => repo.Find(_organisation.Guid))
            .ReturnsAsync(_organisation);

        _mockPersonRepository.Setup(repo => repo.Find(_person.Guid))
            .ReturnsAsync(_person);

        var orgPersonList = new List<OrganisationPerson>() { GetOrganisationPerson() };

        _mockOrganisationRepository.Setup(repo => repo.FindOrganisationPersons(_organisation.Guid))
            .ReturnsAsync(orgPersonList);

        var result = await _useCase.Execute((_organisation.Guid, supportUpdateOrganisation));

        result.Should().BeTrue();
        _organisation.ApprovedOn.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        _organisation.ReviewedBy.Should().Be(_person);
        _organisation.Roles.Should().BeEquivalentTo([PartyRole.Tenderer, PartyRole.Buyer]);
        _organisation.PendingRoles.Should().BeEmpty();
        _organisation.ReviewComment.Should().BeNull();

        _notifyApiClient.Verify(x => x.SendEmail(It.Is<EmailNotificationRequest>(request =>
            request.TemplateId.Contains("buyer-approval-template-id")
            && request.EmailAddress == orgPersonList.First().Person.Email
            && request.Personalisation != null
            && request.Personalisation["org_name"] == _organisation.Name
            && request.Personalisation["first_name"] == orgPersonList.First().Person.FirstName
            && request.Personalisation["last_name"] == orgPersonList.First().Person.LastName
            && request.Personalisation["org_link"].Contains($"organisation/{_organisation.Guid}")
        )));

        _notifyApiClient.Verify(x => x.SendEmail(It.Is<EmailNotificationRequest>(request =>
            request.TemplateId.Contains("buyer-approval-template-id")
            && request.EmailAddress == _organisation.ContactPoints.First().Email
            && request.Personalisation != null
            && request.Personalisation["org_name"] == _organisation.Name
            && request.Personalisation["first_name"] == "organisation"
            && request.Personalisation["last_name"] == "owner"
            && request.Personalisation["org_link"].Contains($"organisation/{_organisation.Guid}")
        )));

        _mockOrganisationRepository.Verify(repo => repo.Save(_organisation), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenUpdateIsReviewAndRejectedAndEmailIsSent_ShouldUpdateOrganisationReviewDetails()
    {
        var supportUpdateOrganisation = new SupportUpdateOrganisation
        {
            Organisation = new SupportOrganisationInfo
            {
                ReviewedById = _person.Guid,
                Approved = false,
                Comment = "Rejected"
            },
            Type = SupportOrganisationUpdateType.Review
        };

        _mockOrganisationRepository.Setup(repo => repo.Find(_organisation.Guid))
            .ReturnsAsync(_organisation);

        _mockPersonRepository.Setup(repo => repo.Find(_person.Guid))
            .ReturnsAsync(_person);

        var orgPersonList = new List<OrganisationPerson>() { GetOrganisationPerson() };

        _mockOrganisationRepository.Setup(repo => repo.FindOrganisationPersons(_organisation.Guid))
            .ReturnsAsync(orgPersonList);

        var result = await _useCase.Execute((_organisation.Guid, supportUpdateOrganisation));

        result.Should().BeTrue();
        _organisation.ReviewedBy.Should().Be(_person);
        _organisation.ReviewComment.Should().Be("Rejected");
        _organisation.PendingRoles.Should().BeEquivalentTo([PartyRole.Buyer]);
        _organisation.Roles.Should().BeEquivalentTo([PartyRole.Tenderer]);

        _notifyApiClient.Verify(x => x.SendEmail(It.Is<EmailNotificationRequest>(request =>
            request.TemplateId.Contains("buyer-rejection-template-id")
            && request.EmailAddress == orgPersonList.First().Person.Email
            && request.Personalisation != null
            && request.Personalisation["org_name"] == _organisation.Name
            && request.Personalisation["first_name"] == orgPersonList.First().Person.FirstName
            && request.Personalisation["last_name"] == orgPersonList.First().Person.LastName
            && request.Personalisation["org_link"].Contains($"organisation/{_organisation.Guid}")
        )));

        _notifyApiClient.Verify(x => x.SendEmail(It.Is<EmailNotificationRequest>(request =>
            request.TemplateId.Contains("buyer-rejection-template-id")
            && request.EmailAddress == _organisation.ContactPoints.First().Email
            && request.Personalisation != null
            && request.Personalisation["org_name"] == _organisation.Name
            && request.Personalisation["first_name"] == "organisation"
            && request.Personalisation["last_name"] == "owner"
            && request.Personalisation["org_link"].Contains($"organisation/{_organisation.Guid}")
        )));

        _mockOrganisationRepository.Verify(repo => repo.Save(_organisation), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenUpdateIsReviewAndNotApproved_ShouldNotSetApprovalDateButStillSave()
    {
        var supportUpdateOrganisation = new SupportUpdateOrganisation
        {
            Organisation = new SupportOrganisationInfo
            {
                ReviewedById = _person.Guid,
                Approved = false,
                Comment = "Reviewed but rejected"
            },
            Type = SupportOrganisationUpdateType.Review
        };

        _mockOrganisationRepository.Setup(repo => repo.Find(_organisation.Guid))
            .ReturnsAsync(_organisation);

        _mockPersonRepository.Setup(repo => repo.Find(_person.Guid))
            .ReturnsAsync(_person);

        var result = await _useCase.Execute((_organisation.Guid, supportUpdateOrganisation));

        result.Should().BeTrue();
        _organisation.ApprovedOn.Should().BeNull();
        _organisation.ReviewedBy.Should().Be(_person);
        _organisation.ReviewComment.Should().Be("Reviewed but rejected");
        _organisation.Roles.Should().BeEquivalentTo([PartyRole.Tenderer]);
        _organisation.PendingRoles.Should().BeEquivalentTo([PartyRole.Buyer]);

        _mockOrganisationRepository.Verify(repo => repo.Save(_organisation), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateSupplierInformationCommand_WhenUpdateTypeIsUnknown()
    {
        var supportUpdateOrganisation = new SupportUpdateOrganisation
        {
            Organisation = new SupportOrganisationInfo
            {
                ReviewedById = _person.Guid,
                Approved = false,
                Comment = "Reviewed but rejected"
            },
            Type = (SupportOrganisationUpdateType)999
        };
        _mockOrganisationRepository.Setup(repo => repo.Find(_organisation.Guid))
            .ReturnsAsync(_organisation);

        _mockPersonRepository.Setup(repo => repo.Find(_person.Guid))
            .ReturnsAsync(_person);

        Func<Task> action = async () => await _useCase.Execute((_organisation.Guid, supportUpdateOrganisation));

        await action.Should()
            .ThrowAsync<InvalidSupportUpdateOrganisationCommand>()
            .WithMessage("Unknown support update organisation command type.");
    }

    [Fact]
    public async Task Execute_WhenUpdateIsAdditionalIdentifiers_ShouldUpdateIdentifiersCorrectly()
    {
        var supportUpdateOrganisation = new SupportUpdateOrganisation
        {
            Type = SupportOrganisationUpdateType.AdditionalIdentifiers,
            Organisation = new SupportOrganisationInfo
            {
                AdditionalIdentifiers = new List<OrganisationIdentifier>
                {
                    new OrganisationIdentifier
                    {
                        Id = "12345678",
                        Scheme = "GB-COH",
                        LegalName = "Test Org Ltd"
                    }
                }
            }
        };

        _organisation.Identifiers = new List<Persistence.Organisation.Identifier>
        {
            new Persistence.Organisation.Identifier
            {
                Scheme = "GB-OLD",
                IdentifierId = "1111",
                LegalName = "Old Org Ltd",
                Primary = false
            }
        };

        _mockOrganisationRepository.Setup(repo => repo.Find(_organisation.Guid))
            .ReturnsAsync(_organisation);

        var result = await _useCase.Execute((_organisation.Guid, supportUpdateOrganisation));

        result.Should().BeTrue();

        _organisation.Identifiers.Should().ContainSingle(i => i.Scheme == "GB-COH" && i.IdentifierId == "12345678" && i.LegalName == "Test Org Ltd");
        _organisation.Identifiers.Should().NotContain(i => i.Scheme == "GB-OLD");

        _mockOrganisationRepository.Verify(repo => repo.Save(_organisation), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenUpdateIsAdditionalIdentifiers_AndMissingIdentifiers_ShouldThrow()
    {
        var supportUpdateOrganisation = new SupportUpdateOrganisation
        {
            Type = SupportOrganisationUpdateType.AdditionalIdentifiers,
            Organisation = new SupportOrganisationInfo
            {
                AdditionalIdentifiers = null
            }
        };

        _mockOrganisationRepository.Setup(repo => repo.Find(_organisation.Guid))
            .ReturnsAsync(_organisation);

        Func<Task> action = async () => await _useCase.Execute((_organisation.Guid, supportUpdateOrganisation));

        await action.Should().ThrowAsync<InvalidUpdateOrganisationCommand.MissingAdditionalIdentifiers>();
    }

    [Fact]
    public async Task Execute_WhenAdditionalIdentifiersAlreadyExist_ShouldUpdateRatherThanDuplicate()
    {
        var yesterday = DateTimeOffset.UtcNow.AddDays(-1);

        var existingIdentifier = new Persistence.Organisation.Identifier
        {
            Scheme = "GB-COH",
            IdentifierId = "old-id",
            LegalName = "Old Legal Name",
            CreatedOn = yesterday,
            UpdatedOn = yesterday,
            Primary = false
        };

        _organisation.Identifiers = new List<Persistence.Organisation.Identifier> { existingIdentifier };

        var supportUpdateOrganisation = new SupportUpdateOrganisation
        {
            Type = SupportOrganisationUpdateType.AdditionalIdentifiers,
            Organisation = new SupportOrganisationInfo
            {
                AdditionalIdentifiers = new List<OrganisationIdentifier>
                {
                    new OrganisationIdentifier
                    {
                        Scheme = "GB-COH",
                        Id = "new-id",
                        LegalName = "New Legal Name"
                    }
                }
            }
        };

        _mockOrganisationRepository.Setup(repo => repo.Find(_organisation.Guid))
            .ReturnsAsync(_organisation);

        var result = await _useCase.Execute((_organisation.Guid, supportUpdateOrganisation));

        result.Should().BeTrue();
        _organisation.Identifiers.Count.Should().Be(1);
        _organisation.Identifiers[0].IdentifierId.Should().Be("new-id");
        _organisation.Identifiers[0].LegalName.Should().Be("New Legal Name");

        _mockOrganisationRepository.Verify(repo => repo.Save(_organisation), Times.Once);
    }


    private static OrganisationPerson GetOrganisationPerson()
    {
        return new OrganisationPerson
        {
            Organisation = Mock.Of<Persistence.Organisation>(),
            Person = Mock.Of<Persistence.Person>(),
            Scopes = ["ADMIN"]
        };
    }
}
