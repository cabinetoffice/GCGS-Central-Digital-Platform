using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class UpdateJoinRequestUseCaseTests
{
    private readonly Mock<IOrganisationRepository> _mockOrganisationRepository;
    private readonly Mock<IOrganisationJoinRequestRepository> _mockRequestRepository;
    private readonly Mock<IPersonRepository> _mockPersonRepository;
    private readonly Mock<IGovUKNotifyApiClient> _mockGovUKNotifyApiClient = new();
    private readonly Mock<ILogger<UpdateJoinRequestUseCase>> _logger = new();
    private readonly IConfiguration _mockConfiguration;
    private readonly UpdateJoinRequestUseCase _useCase;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _joinRequestId = Guid.NewGuid();
    private readonly Guid _reviewedBy = Guid.NewGuid();
    private readonly OrganisationInformation.Persistence.Person _person;
    public UpdateJoinRequestUseCaseTests()
    {
        _mockOrganisationRepository = new Mock<IOrganisationRepository>();
        _mockRequestRepository = new Mock<IOrganisationJoinRequestRepository>();
        _mockPersonRepository = new Mock<IPersonRepository>();

        var inMemorySettings = new List<KeyValuePair<string, string?>>
        {            
            new("OrganisationAppUrl", "http://baseurl/"),
            new("GOVUKNotify:RequestToJoinOrganisationDecisionTemplateId", "template-id")
        };

        _mockConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _useCase = new UpdateJoinRequestUseCase(
                        _mockOrganisationRepository.Object,
                        _mockRequestRepository.Object,
                        _mockPersonRepository.Object,
                        _mockGovUKNotifyApiClient.Object,
                        _mockConfiguration,
                        _logger.Object);

        _person = new OrganisationInformation.Persistence.Person
        {
            Id = 1,
            Guid = _reviewedBy,
            FirstName = "test",
            LastName = "ts",
            Email = "test@ts.com"
        };

    }

    [Fact]
    public async Task Execute_ShouldUpdateJoinRequest_WhenOrganisationAndJoinRequestExist()
    {
        var updateJoinRequest = new UpdateJoinRequest
        {
            ReviewedBy = _reviewedBy,
            status = OrganisationJoinRequestStatus.Accepted,
        };

        var joinRequest = OrganisationJoinRequest;

        _mockOrganisationRepository
            .Setup(repo => repo.Find(_organisationId))
            .ReturnsAsync(Organisation);

        _mockRequestRepository
            .Setup(repo => repo.Find(_joinRequestId, _organisationId))
            .ReturnsAsync(joinRequest);

        _mockPersonRepository.Setup(repo => repo.Find(_person.Guid))
            .ReturnsAsync(_person);

        _mockGovUKNotifyApiClient.Setup(client => client.SendEmail(It.IsAny<EmailNotificationRequest>()));

        var result = await _useCase.Execute((_organisationId, _joinRequestId, updateJoinRequest));

        result.Should().BeTrue();
        joinRequest.ReviewedById.Should().Be(_person.Id);
        joinRequest.Status.Should().Be(updateJoinRequest.status);
        joinRequest.ReviewedOn.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));

        _mockOrganisationRepository.Verify(repo => repo.Find(_organisationId), Times.Once);
        _mockRequestRepository.Verify(repo => repo.Find(_joinRequestId, _organisationId), Times.Once);
        _mockRequestRepository.Verify(repo => repo.Save(joinRequest), Times.Once);
        _mockGovUKNotifyApiClient.Verify(client => client.SendEmail(It.IsAny<EmailNotificationRequest>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenOrganisationNotFound()
    {
        var organisationId = Guid.NewGuid();
        var joinRequestId = Guid.NewGuid();
        var updateJoinRequest = new UpdateJoinRequest
        {
            ReviewedBy = _reviewedBy,
            status = OrganisationJoinRequestStatus.Accepted,
        };

        _mockOrganisationRepository
            .Setup(repo => repo.Find(organisationId))
            .ReturnsAsync((CO.CDP.OrganisationInformation.Persistence.Organisation?)null);

        Func<Task> act = async () => await _useCase.Execute((organisationId, joinRequestId, updateJoinRequest));

        await act.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {organisationId}.");

        _mockOrganisationRepository.Verify(repo => repo.Find(organisationId), Times.Once);
        _mockRequestRepository.Verify(repo => repo.Find(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        _mockRequestRepository.Verify(repo => repo.Save(It.IsAny<CO.CDP.OrganisationInformation.Persistence.OrganisationJoinRequest>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenJoinRequestNotFound()
    {
        var organisationId = Guid.NewGuid();
        var joinRequestId = Guid.NewGuid();
        var updateJoinRequest = new UpdateJoinRequest
        {
            ReviewedBy = _reviewedBy,
            status = OrganisationJoinRequestStatus.Accepted
        };

        _mockOrganisationRepository
            .Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(Organisation);

        _ = _mockRequestRepository
            .Setup(repo => repo.Find(joinRequestId, organisationId))
            .ReturnsAsync((CO.CDP.OrganisationInformation.Persistence.OrganisationJoinRequest?)null);

        Func<Task> act = async () => await _useCase.Execute((organisationId, joinRequestId, updateJoinRequest));

        await act.Should().ThrowAsync<UnknownOrganisationJoinRequestException>()
            .WithMessage($"Unknown organisation join request for org id {organisationId} or request id {joinRequestId}.");

        _mockOrganisationRepository.Verify(repo => repo.Find(organisationId), Times.Once);
        _mockRequestRepository.Verify(repo => repo.Find(joinRequestId, organisationId), Times.Once);
        _mockRequestRepository.Verify(repo => repo.Save(It.IsAny<CO.CDP.OrganisationInformation.Persistence.OrganisationJoinRequest>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenPersonNotFound()
    {
        var updateJoinRequest = new UpdateJoinRequest
        {
            ReviewedBy = _reviewedBy,
            status = OrganisationJoinRequestStatus.Accepted,
        };

        var joinRequest = OrganisationJoinRequest;

        _mockOrganisationRepository
            .Setup(repo => repo.Find(_organisationId))
            .ReturnsAsync(Organisation);

        _mockRequestRepository
            .Setup(repo => repo.Find(_joinRequestId, _organisationId))
            .ReturnsAsync(joinRequest);

        _mockPersonRepository.Setup(repo => repo.Find(_person.Guid))
            .ReturnsAsync((CO.CDP.OrganisationInformation.Persistence.Person?)null);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, _joinRequestId, updateJoinRequest));

        await act.Should().ThrowAsync<UnknownPersonException>()
            .WithMessage($"Unknown person {_person.Guid}.");

        _mockOrganisationRepository.Verify(repo => repo.Find(_organisationId), Times.Once);
        _mockRequestRepository.Verify(repo => repo.Find(_joinRequestId, _organisationId), Times.Once);
        _mockRequestRepository.Verify(repo => repo.Save(It.IsAny<CO.CDP.OrganisationInformation.Persistence.OrganisationJoinRequest>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ShouldAddPersonToOrganisation_WhenStatusIsAccepted()
    {
        var organisation = Organisation;
        var joinRequest = OrganisationJoinRequest;
        var person = _person;

        var updateJoinRequest = new UpdateJoinRequest
        {
            ReviewedBy = _reviewedBy,
            status = OrganisationJoinRequestStatus.Accepted,
        };

        _mockOrganisationRepository.Setup(r => r.Find(_organisationId)).ReturnsAsync(organisation);
        _mockRequestRepository.Setup(r => r.Find(_joinRequestId, _organisationId)).ReturnsAsync(joinRequest);
        _mockPersonRepository.Setup(r => r.Find(_reviewedBy)).ReturnsAsync(person);

        var result = await _useCase.Execute((_organisationId, _joinRequestId, updateJoinRequest));

        result.Should().BeTrue();
        organisation.OrganisationPersons.Should().Contain(op => op.Person.Guid == _reviewedBy);
        _mockOrganisationRepository.Verify(r => r.Save(organisation), Times.Once);
        _mockRequestRepository.Verify(r => r.Save(joinRequest), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldNotAddPerson_WhenStatusIsRejected()
    {

        var organisation = Organisation;
        var joinRequest = OrganisationJoinRequest;
        var person = _person;

        var updateJoinRequest = new UpdateJoinRequest
        {
            ReviewedBy = _reviewedBy,
            status = OrganisationJoinRequestStatus.Rejected,
        };

        joinRequest.Status = OrganisationJoinRequestStatus.Rejected;
        _mockOrganisationRepository.Setup(r => r.Find(_organisationId)).ReturnsAsync(organisation);
        _mockRequestRepository.Setup(r => r.Find(_joinRequestId, _organisationId)).ReturnsAsync(joinRequest);
        _mockPersonRepository.Setup(r => r.Find(_person.Guid)).ReturnsAsync(_person);

        var result = await _useCase.Execute((_organisationId, _joinRequestId, updateJoinRequest));

        result.Should().BeTrue();
        Organisation.OrganisationPersons.Should().BeEmpty();
        _mockOrganisationRepository.Verify(r => r.Save(organisation), Times.Never);
        _mockRequestRepository.Verify(r => r.Save(joinRequest), Times.Once);
    }

    private CO.CDP.OrganisationInformation.Persistence.Organisation Organisation =>
        new CO.CDP.OrganisationInformation.Persistence.Organisation
        {
            Guid = _organisationId,
            Name = "Test",
            Tenant = It.IsAny<Tenant>(),
            ContactPoints = [new CO.CDP.OrganisationInformation.Persistence.Organisation.ContactPoint { Email = "test@test.com" }],
            SupplierInfo = new CO.CDP.OrganisationInformation.Persistence.Organisation.SupplierInformation()
        };

    private CO.CDP.OrganisationInformation.Persistence.OrganisationJoinRequest OrganisationJoinRequest =>
        new CO.CDP.OrganisationInformation.Persistence.OrganisationJoinRequest
        {
            Guid = _joinRequestId,
            Status = OrganisationJoinRequestStatus.Accepted,
            Organisation = Organisation,
            Id = 1,
            Person = _person
        };
}