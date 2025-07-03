using CO.CDP.Authentication;
using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class AddOrganisationPartyUseCaseTests
{
    private readonly Mock<IOrganisationRepository> _orgRepoMock = new();
    private readonly Mock<IShareCodeRepository> _shareCodeRepoMock = new();
    private readonly Mock<IOrganisationPartiesRepository> _orgPartiesRepoMock = new();
    private readonly Mock<IGovUKNotifyApiClient> _notifyClientMock = new();
    private readonly Mock<IClaimService> _claimServiceMock = new();
    private readonly Mock<IPersonRepository> _personRepoMock = new();
    private readonly Mock<ILogger<AddOrganisationPartyUseCase>> _loggerMock = new();
    private readonly IConfiguration _config;

    private AddOrganisationPartyUseCase UseCase => new(
        _orgRepoMock.Object, _shareCodeRepoMock.Object, _orgPartiesRepoMock.Object, _notifyClientMock.Object,
        _config, _claimServiceMock.Object, _personRepoMock.Object, _loggerMock.Object);

    public AddOrganisationPartyUseCaseTests()
    {
        var inMemorySettings = new List<KeyValuePair<string, string?>>
        {
            new("GOVUKNotify:ConsortiumOrganisationAddedEmailTemplateId", "test-template-id")
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenParentOrganisationNotFound()
    {
        var organisationId = Guid.NewGuid();
        var addParty = new AddOrganisationParty
        {
            OrganisationPartyId = Guid.NewGuid(),
            ShareCode = null,
            OrganisationRelationship = Model.OrganisationRelationship.Consortium,
        };

        _orgRepoMock.Setup(repo => repo.Find(organisationId)).ReturnsAsync((Persistence.Organisation?)null);

        Func<Task> action = async () => await UseCase.Execute((organisationId, addParty));

        await action.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {organisationId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenChildOrganisationNotFound()
    {
        var organisationId = Guid.NewGuid();
        var parentOrg = GivenOrganisation(organisationId);
        var childOrganisationId = Guid.NewGuid();
        var addParty = new AddOrganisationParty
        {
            OrganisationPartyId = childOrganisationId,
            ShareCode = null,
            OrganisationRelationship = Model.OrganisationRelationship.Consortium,
        };

        _orgRepoMock.Setup(repo => repo.Find(organisationId)).ReturnsAsync(parentOrg);
        _orgRepoMock.Setup(repo => repo.Find(childOrganisationId)).ReturnsAsync((Persistence.Organisation?)null);

        Func<Task> action = async () => await UseCase.Execute((organisationId, addParty));

        await action.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {childOrganisationId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenShareCodeIsInvalid()
    {
        var organisationId = Guid.NewGuid();
        var parentOrg = GivenOrganisation(organisationId);
        var childOrganisationId = Guid.NewGuid();
        var childOrg = GivenOrganisation(childOrganisationId);
        var shareCode = "InvalidCode";
        var addParty = new AddOrganisationParty
        {
            OrganisationPartyId = childOrganisationId,
            ShareCode = shareCode,
            OrganisationRelationship = Model.OrganisationRelationship.Consortium,
        };

        _orgRepoMock.Setup(repo => repo.Find(organisationId)).ReturnsAsync(parentOrg);
        _orgRepoMock.Setup(repo => repo.Find(childOrganisationId)).ReturnsAsync(childOrg);
        _shareCodeRepoMock.Setup(repo => repo.GetShareCodesAsync(childOrganisationId)).ReturnsAsync([]);

        Func<Task> action = async () => await UseCase.Execute((organisationId, addParty));

        await action.Should().ThrowAsync<OrganisationShareCodeInvalid>()
            .WithMessage($"Invalid organisation share code: {shareCode}");
    }

    [Fact]
    public async Task Execute_ShouldSaveOrganisationPartyAndSendNotificationEmail_WhenValidInputProvided()
    {
        var organisationId = Guid.NewGuid();
        var parentOrg = GivenOrganisation(organisationId, "Parent Org");
        var childOrganisationId = Guid.NewGuid();
        var childOrg = GivenOrganisation(childOrganisationId, "Child Org", "child_org@test.com");
        var shareCode = "ValidCode";
        var sharedConsent = new Persistence.Forms.SharedConsent
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            OrganisationId = childOrg.Id,
            Organisation = childOrg,
            FormId = 1,
            Form = null!,
            AnswerSets = [],
            SubmissionState = Persistence.Forms.SubmissionState.Submitted,
            SubmittedAt = null,
            FormVersionId = string.Empty,
            ShareCode = shareCode ?? "valid-sharecode",
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow,
        };
        var adminPerson = new OrganisationPerson
        {
            Person = new Persistence.Person { Email = "admin@org.com", FirstName = "Alice", LastName = "Smith", Guid = Guid.Empty, UserUrn = "" }
        };
        var user = new Persistence.Person { FirstName = "Delta", LastName = "Hero", Email = "", Guid = Guid.Empty, UserUrn = "" };
        var capturedRequests = new List<EmailNotificationRequest>();

        _orgRepoMock.Setup(repo => repo.Find(organisationId)).ReturnsAsync(parentOrg);
        _orgRepoMock.Setup(repo => repo.Find(childOrganisationId)).ReturnsAsync(childOrg);
        _shareCodeRepoMock.Setup(repo => repo.GetShareCodesAsync(childOrganisationId)).ReturnsAsync([sharedConsent]);
        _orgRepoMock.Setup(r => r.FindOrganisationPersons(childOrg.Guid, It.IsAny<IEnumerable<string>>())).ReturnsAsync([adminPerson]);
        _claimServiceMock.Setup(c => c.GetUserUrn()).Returns("user-urn");
        _personRepoMock.Setup(r => r.FindByUrn("user-urn")).ReturnsAsync(user);
        _notifyClientMock.Setup(c => c.SendEmail(It.IsAny<EmailNotificationRequest>()))
                         .Callback<EmailNotificationRequest>(capturedRequests.Add)
                         .ReturnsAsync((EmailNotificationResponse?)null);

        var result = await UseCase.Execute((organisationId, new AddOrganisationParty
        {
            OrganisationPartyId = childOrganisationId,
            ShareCode = shareCode,
            OrganisationRelationship = Model.OrganisationRelationship.Consortium,
        }));

        result.Should().BeTrue();

        _orgPartiesRepoMock.Verify(repo => repo.Save(It.Is<Persistence.OrganisationParty>(party =>
            party.ParentOrganisationId == parentOrg.Id &&
            party.ChildOrganisationId == childOrg.Id &&
            party.SharedConsentId == sharedConsent.Id &&
            party.OrganisationRelationship == Persistence.OrganisationRelationship.Consortium)), Times.Once);

        capturedRequests.Should().HaveCount(2);

        capturedRequests.Should().ContainSingle(r =>
            r.EmailAddress == "admin@org.com" &&
            r.TemplateId == "test-template-id" &&
            r.Personalisation!["org_name"] == "Child Org" &&
            r.Personalisation!["first_name"] == "Alice" &&
            r.Personalisation["last_name"] == "Smith" &&
            r.Personalisation["consortium_name"] == "Parent Org" &&
            r.Personalisation["person_who_added_it"] == "Delta Hero"
        );

        capturedRequests.Should().ContainSingle(r =>
            r.EmailAddress == "child_org@test.com" &&
            r.Personalisation!["first_name"] == "organisation" &&
            r.Personalisation["last_name"] == "owner"
        );
    }

    private static Persistence.Organisation GivenOrganisation(Guid guid, string name = "Test", string email = "test@test.com") =>
        new()
        {
            Guid = guid,
            Name = name,
            Type = OrganisationInformation.OrganisationType.Organisation,
            Tenant = It.IsAny<Tenant>(),
            ContactPoints = [new Persistence.ContactPoint { Email = email }],
            SupplierInfo = new Persistence.SupplierInformation()
        };
}