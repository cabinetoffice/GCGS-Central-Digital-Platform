using CO.CDP.Authentication;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class CreateParentChildRelationshipUseCaseTest
{
    private readonly Mock<IClaimService> _claimService = new();
    private readonly Mock<ILogger<CreateParentChildRelationshipUseCase>> _logger = new();
    private readonly Mock<IOrganisationHierarchyRepository> _repository = new();

    private CreateParentChildRelationshipUseCase UseCase => new(
        _logger.Object,
        _repository.Object,
        _claimService.Object);

    [Fact]
    public async Task Execute_WithValidParameters_ShouldCreateRelationship()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var relationshipId = Guid.NewGuid();
        var userUrn = "urn:fdc:gov.uk:2022:test-user";

        _claimService.Setup(c => c.GetUserUrn()).Returns(userUrn);
        _repository
            .Setup(r => r.CreateRelationshipAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string?>()))
            .ReturnsAsync(relationshipId);
        var request = new CreateParentChildRelationshipRequest
        {
            ParentId = parentId,
            ChildId = childId
        };

        var result = await UseCase.Execute(request);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.RelationshipId.Should().Be(relationshipId);

        _repository.Verify(r => r.CreateRelationshipAsync(
                parentId,
                childId,
                userUrn),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WithValidParameters_ShouldPassUserUrnAsCreatedBy()
    {
        var userUrn = "urn:fdc:gov.uk:2022:audited-user";
        _claimService.Setup(c => c.GetUserUrn()).Returns(userUrn);
        _repository
            .Setup(r => r.CreateRelationshipAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>()))
            .ReturnsAsync(Guid.NewGuid());

        var request = new CreateParentChildRelationshipRequest
        {
            ParentId = Guid.NewGuid(),
            ChildId = Guid.NewGuid()
        };

        await UseCase.Execute(request);

        _repository.Verify(r => r.CreateRelationshipAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                userUrn),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WithEmptyParentId_ShouldReturnFailure()
    {
        var request = new CreateParentChildRelationshipRequest
        {
            ParentId = Guid.Empty,
            ChildId = Guid.NewGuid()
        };

        var result = await UseCase.Execute(request);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.RelationshipId.Should().BeNull();

        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid organisation IDs provided")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
            ), Times.Once);
    }

    [Fact]
    public async Task Execute_WithEmptyChildId_ShouldReturnFailure()
    {
        var request = new CreateParentChildRelationshipRequest
        {
            ParentId = Guid.NewGuid(),
            ChildId = Guid.Empty
        };

        var result = await UseCase.Execute(request);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.RelationshipId.Should().BeNull();

        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid organisation IDs provided")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
            ), Times.Once);
    }

    [Fact]
    public async Task Execute_WithSameParentAndChildId_ShouldReturnFailure()
    {
        var sameId = Guid.NewGuid();
        var request = new CreateParentChildRelationshipRequest
        {
            ParentId = sameId,
            ChildId = sameId
        };

        var result = await UseCase.Execute(request);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.RelationshipId.Should().BeNull();

        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Parent and child organisation IDs cannot be the same")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
            ), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenExceptionOccurs_ShouldReturnFailure()
    {
        var mockLogger = new Mock<ILogger<CreateParentChildRelationshipUseCase>>();
        mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            )
        ).Throws(new Exception("Simulated exception"));

        var mockRepository = new Mock<IOrganisationHierarchyRepository>();
        var mockClaimService = new Mock<IClaimService>();
        mockClaimService.Setup(c => c.GetUserUrn()).Returns("urn:fdc:gov.uk:2022:test-user");

        var useCase = new CreateParentChildRelationshipUseCase(
            mockLogger.Object,
            mockRepository.Object,
            mockClaimService.Object);

        var request = new CreateParentChildRelationshipRequest
        {
            ParentId = Guid.NewGuid(),
            ChildId = Guid.NewGuid()
        };

        var result = await useCase.Execute(request);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.RelationshipId.Should().BeNull();

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error creating parent-child relationship")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
            ), Times.Once);
    }
}