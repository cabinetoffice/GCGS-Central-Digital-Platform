using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class SupersedeChildOrganisationUseCaseTests
{
    private readonly Mock<IOrganisationHierarchyRepository> _hierarchyRepositoryMock;
    private readonly SupersedeChildOrganisationUseCase _useCase;

    public SupersedeChildOrganisationUseCaseTests()
    {
        var loggerMock = new Mock<ILogger<SupersedeChildOrganisationUseCase>>();
        _hierarchyRepositoryMock = new Mock<IOrganisationHierarchyRepository>();
        _useCase = new SupersedeChildOrganisationUseCase(loggerMock.Object, _hierarchyRepositoryMock.Object);
    }

    [Fact]
    public async Task Execute_WhenRelationshipNotFound_ReturnsNotFoundResult()
    {
        var request = new SupersedeChildOrganisationRequest
        {
            ParentOrganisationId = Guid.NewGuid(),
            ChildOrganisationId = Guid.NewGuid()
        };

        _hierarchyRepositoryMock.Setup(r => r.GetChildrenAsync(request.ParentOrganisationId))
            .ReturnsAsync([]);

        var result = await _useCase.Execute(request);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.NotFound.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_WhenRelationshipFoundAndSuperseded_ReturnsSuccessfulResult()
    {
        var request = new SupersedeChildOrganisationRequest
        {
            ParentOrganisationId = Guid.NewGuid(),
            ChildOrganisationId = Guid.NewGuid()
        };

        var relationship = new OrganisationHierarchy
        {
            RelationshipId = Guid.NewGuid(),
            Child = new OrganisationInformation.Persistence.Organisation
            {
                Guid = request.ChildOrganisationId,
                Name = "Test Child Organisation",
                Tenant = new Tenant { Guid = Guid.NewGuid(), Name = "Test Tenant" },
                Type = OrganisationType.Organisation,
                Roles = [],
                CreatedOn = DateTimeOffset.UtcNow,
                UpdatedOn = DateTimeOffset.UtcNow
            },
            ParentOrganisationId = 1,
            Roles = [],
            CreatedOn = DateTimeOffset.UtcNow
        };

        _hierarchyRepositoryMock.Setup(r => r.GetChildrenAsync(request.ParentOrganisationId))
            .ReturnsAsync([relationship]);

        _hierarchyRepositoryMock.Setup(r => r.SupersedeRelationshipAsync(relationship.RelationshipId))
            .ReturnsAsync(true);

        var result = await _useCase.Execute(request);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.NotFound.Should().BeFalse();
        result.SupersededDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Execute_WhenRelationshipFoundButFailsToSupersede_ReturnsUnsuccessfulResult()
    {
        var request = new SupersedeChildOrganisationRequest
        {
            ParentOrganisationId = Guid.NewGuid(),
            ChildOrganisationId = Guid.NewGuid()
        };

        var relationship = new OrganisationHierarchy
        {
            RelationshipId = Guid.NewGuid(),
            Child = new OrganisationInformation.Persistence.Organisation
            {
                Guid = request.ChildOrganisationId,
                Name = "Test Child Organisation",
                Tenant = new Tenant { Guid = Guid.NewGuid(), Name = "Test Tenant" },
                Type = OrganisationType.Organisation,
                Roles = [],
                CreatedOn = DateTimeOffset.UtcNow,
                UpdatedOn = DateTimeOffset.UtcNow
            },
            ParentOrganisationId = 1,
            Roles = [],
            CreatedOn = DateTimeOffset.UtcNow
        };

        _hierarchyRepositoryMock.Setup(r => r.GetChildrenAsync(request.ParentOrganisationId))
            .ReturnsAsync([relationship]);

        _hierarchyRepositoryMock.Setup(r => r.SupersedeRelationshipAsync(relationship.RelationshipId))
            .ReturnsAsync(false);

        var result = await _useCase.Execute(request);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.NotFound.Should().BeFalse();
        result.SupersededDate.Should().BeNull();
    }

    [Fact]
    public async Task Execute_WhenExceptionThrown_ReturnsUnsuccessfulResult()
    {
        var request = new SupersedeChildOrganisationRequest
        {
            ParentOrganisationId = Guid.NewGuid(),
            ChildOrganisationId = Guid.NewGuid()
        };

        _hierarchyRepositoryMock.Setup(r => r.GetChildrenAsync(request.ParentOrganisationId))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _useCase.Execute(request);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.NotFound.Should().BeFalse();
    }
}