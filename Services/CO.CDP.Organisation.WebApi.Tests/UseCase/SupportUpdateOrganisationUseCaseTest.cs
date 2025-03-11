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
        var organisation = GivenOrganisation([PartyRole.Buyer], [PartyRole.Tenderer]);
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

        _mockOrganisationRepository.Setup(repo => repo.Find(organisation.Guid))
            .ReturnsAsync(organisation);

        _mockPersonRepository.Setup(repo => repo.Find(personId))
            .ReturnsAsync(null as Persistence.Person);

        Func<Task> action = async () => await _useCase.Execute((organisation.Guid, supportUpdateOrganisation));

        await action.Should().ThrowAsync<UnknownPersonException>()
            .WithMessage($"Unknown person {personId}.");
    }

    [Fact]
    public async Task Execute_WhenUpdateIsReviewAndApprovedAndEmailIsSent_ShouldUpdateOrganisationReviewDetails()
    {
        var organisation = GivenOrganisation([PartyRole.Buyer], [PartyRole.Tenderer]);
        var supportUpdateOrganisation = new SupportUpdateOrganisation
        {
            Organisation = new SupportOrganisationInfo
            {
                ReviewedById = _person.Guid,
                Approved = true
            },
            Type = SupportOrganisationUpdateType.Review
        };

        _mockOrganisationRepository.Setup(repo => repo.Find(organisation.Guid))
            .ReturnsAsync(organisation);

        _mockPersonRepository.Setup(repo => repo.Find(_person.Guid))
            .ReturnsAsync(_person);

        var orgPersonList = new List<OrganisationPerson>() { GetOrganisationPerson() };

        _mockOrganisationRepository.Setup(repo => repo.FindOrganisationPersons(organisation.Guid))
            .ReturnsAsync(orgPersonList);

        var result = await _useCase.Execute((organisation.Guid, supportUpdateOrganisation));

        result.Should().BeTrue();
        organisation.ApprovedOn.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        organisation.ReviewedBy.Should().Be(_person);
        organisation.Roles.Should().BeEquivalentTo([PartyRole.Tenderer, PartyRole.Buyer]);
        organisation.PendingRoles.Should().BeEmpty();
        organisation.ReviewComment.Should().BeNull();

        _notifyApiClient.Verify(x => x.SendEmail(It.Is<EmailNotificationRequest>(request =>
            request.TemplateId.Contains("buyer-approval-template-id")
            && request.EmailAddress == orgPersonList.First().Person.Email
            && request.Personalisation != null
            && request.Personalisation["org_name"] == organisation.Name
            && request.Personalisation["first_name"] == orgPersonList.First().Person.FirstName
            && request.Personalisation["last_name"] == orgPersonList.First().Person.LastName
            && request.Personalisation["org_link"].Contains($"organisation/{organisation.Guid}")
        )));

        _notifyApiClient.Verify(x => x.SendEmail(It.Is<EmailNotificationRequest>(request =>
            request.TemplateId.Contains("buyer-approval-template-id")
            && request.EmailAddress == organisation.ContactPoints.First().Email
            && request.Personalisation != null
            && request.Personalisation["org_name"] == organisation.Name
            && request.Personalisation["first_name"] == "organisation"
            && request.Personalisation["last_name"] == "owner"
            && request.Personalisation["org_link"].Contains($"organisation/{organisation.Guid}")
        )));

        _mockOrganisationRepository.Verify(repo => repo.Save(organisation), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenUpdateIsReviewAndRejectedAndEmailIsSent_ShouldUpdateOrganisationReviewDetails()
    {
        var organisation = GivenOrganisation([PartyRole.Buyer], [PartyRole.Tenderer]); 
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

        _mockOrganisationRepository.Setup(repo => repo.Find(organisation.Guid))
            .ReturnsAsync(organisation);

        _mockPersonRepository.Setup(repo => repo.Find(_person.Guid))
            .ReturnsAsync(_person);

        var orgPersonList = new List<OrganisationPerson>() { GetOrganisationPerson() };

        _mockOrganisationRepository.Setup(repo => repo.FindOrganisationPersons(organisation.Guid))
            .ReturnsAsync(orgPersonList);

        var result = await _useCase.Execute((organisation.Guid, supportUpdateOrganisation));

        result.Should().BeTrue();
        organisation.ReviewedBy.Should().Be(_person);
        organisation.ReviewComment.Should().Be("Rejected");
        organisation.PendingRoles.Should().BeEquivalentTo([PartyRole.Buyer]);
        organisation.Roles.Should().BeEquivalentTo([PartyRole.Tenderer]);

        _notifyApiClient.Verify(x => x.SendEmail(It.Is<EmailNotificationRequest>(request =>
            request.TemplateId.Contains("buyer-rejection-template-id")
            && request.EmailAddress == orgPersonList.First().Person.Email
            && request.Personalisation != null
            && request.Personalisation["org_name"] == organisation.Name
            && request.Personalisation["first_name"] == orgPersonList.First().Person.FirstName
            && request.Personalisation["last_name"] == orgPersonList.First().Person.LastName
            && request.Personalisation["org_link"].Contains($"organisation/{organisation.Guid}")
        )));

        _notifyApiClient.Verify(x => x.SendEmail(It.Is<EmailNotificationRequest>(request =>
            request.TemplateId.Contains("buyer-rejection-template-id")
            && request.EmailAddress == organisation.ContactPoints.First().Email
            && request.Personalisation != null
            && request.Personalisation["org_name"] == organisation.Name
            && request.Personalisation["first_name"] == "organisation"
            && request.Personalisation["last_name"] == "owner"
            && request.Personalisation["org_link"].Contains($"organisation/{organisation.Guid}")
        )));

        _mockOrganisationRepository.Verify(repo => repo.Save(organisation), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenUpdateIsReviewAndNotApproved_ShouldNotSetApprovalDateButStillSave()
    {
        var organisation = GivenOrganisation([PartyRole.Buyer], [PartyRole.Tenderer]);
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

        _mockOrganisationRepository.Setup(repo => repo.Find(organisation.Guid))
            .ReturnsAsync(organisation);

        _mockPersonRepository.Setup(repo => repo.Find(_person.Guid))
            .ReturnsAsync(_person);

        var result = await _useCase.Execute((organisation.Guid, supportUpdateOrganisation));

        result.Should().BeTrue();
        organisation.ApprovedOn.Should().BeNull();
        organisation.ReviewedBy.Should().Be(_person);
        organisation.ReviewComment.Should().Be("Reviewed but rejected");
        organisation.Roles.Should().BeEquivalentTo([PartyRole.Tenderer]);
        organisation.PendingRoles.Should().BeEquivalentTo([PartyRole.Buyer]);

        _mockOrganisationRepository.Verify(repo => repo.Save(organisation), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateSupplierInformationCommand_WhenUpdateTypeIsUnknown()
    {
        var organisation = GivenOrganisation([PartyRole.Buyer], [PartyRole.Tenderer]);
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
        _mockOrganisationRepository.Setup(repo => repo.Find(organisation.Guid))
            .ReturnsAsync(organisation);

        _mockPersonRepository.Setup(repo => repo.Find(_person.Guid))
            .ReturnsAsync(_person);

        Func<Task> action = async () => await _useCase.Execute((organisation.Guid, supportUpdateOrganisation));

        await action.Should()
            .ThrowAsync<InvalidSupportUpdateOrganisationCommand>()
            .WithMessage("Unknown support update organisation command type.");
    }

    [Fact]
    public async Task Execute_WhenUpdateIsConvertPendingBuyerToSupplier_ShouldConvertPendingBuyerToSupplier()
    {
        var organisation = GivenOrganisation([PartyRole.Buyer], null);
        var supportUpdateOrganisation = new SupportUpdateOrganisation
        {
            Organisation = new SupportOrganisationInfo
            {
                Approved = false,
                ReviewedById = _person.Guid,
                Comment = "Converting pending buyer to supplier"
            },
            Type = SupportOrganisationUpdateType.ConvertPendingBuyerToSupplier
        };

        _mockOrganisationRepository.Setup(repo => repo.Find(organisation.Guid))
            .ReturnsAsync(organisation);

        _mockPersonRepository.Setup(repo => repo.Find(_person.Guid))
            .ReturnsAsync(_person);

        var result = await _useCase.Execute((organisation.Guid, supportUpdateOrganisation));

        result.Should().BeTrue();
        organisation.ApprovedOn.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        organisation.ReviewedBy.Should().Be(_person);
        organisation.ReviewComment.Should().Be("Converting pending buyer to supplier");
        organisation.Roles.Should().BeEquivalentTo(new[] { PartyRole.Tenderer });
        organisation.PendingRoles.Should().BeEmpty();
        organisation.BuyerInfo.Should().BeNull();

        _mockOrganisationRepository.Verify(repo => repo.RemoveMouSignatures(organisation.Id), Times.Once);
        _mockOrganisationRepository.Verify(repo => repo.Save(organisation), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenUpdateIsConvertPendingBuyerToSupplier_ShouldConvertSupplierAndPendingBuyerToJustSupplier()
    {
        var organisation = GivenOrganisation([PartyRole.Buyer], [PartyRole.Tenderer]);
        var supportUpdateOrganisation = new SupportUpdateOrganisation
        {
            Organisation = new SupportOrganisationInfo
            {
                Approved = false,
                ReviewedById = _person.Guid,
                Comment = "Converting pending buyer to supplier"
            },
            Type = SupportOrganisationUpdateType.ConvertPendingBuyerToSupplier
        };

        _mockOrganisationRepository.Setup(repo => repo.Find(organisation.Guid))
            .ReturnsAsync(organisation);

        _mockPersonRepository.Setup(repo => repo.Find(_person.Guid))
            .ReturnsAsync(_person);

        var result = await _useCase.Execute((organisation.Guid, supportUpdateOrganisation));

        result.Should().BeTrue();
        organisation.ApprovedOn.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        organisation.ReviewedBy.Should().Be(_person);
        organisation.ReviewComment.Should().Be("Converting pending buyer to supplier");
        organisation.Roles.Should().BeEquivalentTo(new[] { PartyRole.Tenderer });
        organisation.PendingRoles.Should().BeEmpty();
        organisation.BuyerInfo.Should().BeNull();

        _mockOrganisationRepository.Verify(repo => repo.RemoveMouSignatures(organisation.Id), Times.Once);
        _mockOrganisationRepository.Verify(repo => repo.Save(organisation), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenOrganisationIsNotPendingBuyer_ShouldThrowInvalidSupportUpdateOrganisationCommand()
    {
        var organisation = GivenOrganisation(null, [PartyRole.Tenderer]);

        var supportUpdateOrganisation = new SupportUpdateOrganisation
        {
            Organisation = new SupportOrganisationInfo
            {
                Approved = false,
                ReviewedById = _person.Guid,
                Comment = "Attempting to convert non-pending buyer to supplier"
            },
            Type = SupportOrganisationUpdateType.ConvertPendingBuyerToSupplier
        };

        _mockOrganisationRepository.Setup(repo => repo.Find(organisation.Guid))
            .ReturnsAsync(organisation);

        _mockPersonRepository.Setup(repo => repo.Find(_person.Guid))
            .ReturnsAsync(_person);

        Func<Task> action = async () => await _useCase.Execute((organisation.Guid, supportUpdateOrganisation));

        await action.Should().ThrowAsync<InvalidSupportUpdateOrganisationCommand>()
            .WithMessage("Organisation must be a pending buyer.");
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

    private static Persistence.Organisation GivenOrganisation(List<PartyRole>? pendingRoles = null, List<PartyRole>? roles = null)
    {
        return new Persistence.Organisation
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Roles = roles ?? [],
            PendingRoles = pendingRoles ?? [],
            Tenant = null!,
            Name = null!,
            Type = OrganisationType.Organisation,
            ContactPoints = [new Persistence.Organisation.ContactPoint { Email = "org-email@test.com" }]
        };
    }
}
